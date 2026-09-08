using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpsideDownWorldManager : MonoBehaviour
{
    [Header("References")]

    [Tooltip("Rodzic całego świata, który ma się obracać.")]
    public Transform worldRoot;

    [Tooltip("Główny XR Origin gracza.")]
    public Transform xrOrigin;

    [Tooltip("Main Camera / HMD.")]
    public Transform hmd;

    [Tooltip("Obiekt Locomotion znajdujący się pod XR Origin.")]
    public GameObject locomotionRoot;

    [Tooltip("Punkt w środku pokoju, do którego ma dolecieć GŁOWA gracza.")]
    public Transform playerCenterPoint;


    [Header("Player Levitation")]

    [Tooltip("Czas lewitacji gracza do środka pokoju.")]
    public float playerRiseDuration = 3f;

    [Tooltip("Czas powrotu gracza na poprzednią pozycję.")]
    public float playerReturnDuration = 3f;


    [Header("World Rotation")]

    [Tooltip("Czas obrotu świata 0 → 180.")]
    public float turnDuration = 5f;

    [Tooltip("Czas pozostania świata do góry nogami.")]
    public float upsideDownDuration = 4f;

    [Tooltip("Czas obrotu świata 180 → 360.")]
    public float returnDuration = 5f;


    [Header("Upside Down Wobble")]

    public float wobbleAngle = 1f;
    public float wobbleSpeed = 0.4f;


    [Header("Smoothing")]

    public AnimationCurve movementCurve =
        AnimationCurve.EaseInOut(
            0f, 0f,
            1f, 1f
        );


    Coroutine effectRoutine;

    Transform rotationPivot;

    // =============================
    // WORLD STATE
    // =============================

    Transform originalWorldParent;
    int originalWorldSiblingIndex;

    Vector3 originalWorldLocalPosition;
    Quaternion originalWorldLocalRotation;
    Vector3 originalWorldLocalScale;

    bool worldAttachedToPivot;


    // =============================
    // PLAYER STATE
    // =============================

    Vector3 originalXROriginPosition;

    CharacterController characterController;
    bool characterControllerWasEnabled;

    bool locomotionWasActive;

    bool playerStatePrepared;


    // =============================
    // WORLD PHYSICS
    // =============================

    class RigidbodyState
    {
        public Rigidbody rb;
        public bool wasKinematic;
        public bool usedGravity;
    }

    List<RigidbodyState> rigidbodyStates =
        new List<RigidbodyState>();


    Vector3 rotationAxis;


    // ==================================================
    // START EFFECT
    // ==================================================

    public void StartUpsideDownWorld()
    {
        if (!worldRoot)
        {
            Debug.LogError(
                "UpsideDownWorldManager: brak World Root!"
            );
            return;
        }

        if (!xrOrigin)
        {
            Debug.LogError(
                "UpsideDownWorldManager: brak XR Origin!"
            );
            return;
        }

        if (!hmd)
        {
            Debug.LogError(
                "UpsideDownWorldManager: brak HMD / Main Camera!"
            );
            return;
        }

        if (!playerCenterPoint)
        {
            Debug.LogError(
                "UpsideDownWorldManager: brak Player Center Point!"
            );
            return;
        }


        // Jeżeli efekt już działa,
        // przywróć wszystko przed ponownym uruchomieniem.
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);

            effectRoutine = null;

            RestoreImmediately();
        }


        effectRoutine =
            StartCoroutine(UpsideDownRoutine());
    }


    // ==================================================
    // MAIN ROUTINE
    // ==================================================

    IEnumerator UpsideDownRoutine()
    {
        PreparePlayer();
        PrepareRigidbodies();


        // ===========================================
        // 0–3 SEKUND
        // GRACZ LEWITUJE DO ŚRODKA
        // ===========================================

        Vector3 targetXRPosition =
            CalculateXRPositionForHMDTarget(
                playerCenterPoint.position
            );

        yield return MovePlayer(
            originalXROriginPosition,
            targetXRPosition,
            playerRiseDuration
        );


        // ===========================================
        // TERAZ GRACZ JEST W ŚRODKU.
        // TWORZYMY PIVOT DOKŁADNIE W JEGO POZYCJI.
        // ===========================================

        PrepareWorldPivot();


        // ===========================================
        // 3–8 SEKUND
        // ŚWIAT 0 → 180
        // ===========================================

        yield return RotateWorld(
            0f,
            180f,
            turnDuration
        );


        // ===========================================
        // 8–12 SEKUND
        // ŚWIAT DO GÓRY NOGAMI
        // ===========================================

        float timer = 0f;

        while (timer < upsideDownDuration)
        {
            timer += Time.deltaTime;

            float wobble =
                Mathf.Sin(
                    timer *
                    wobbleSpeed *
                    Mathf.PI *
                    2f
                )
                * wobbleAngle;

            SetWorldAngle(
                180f + wobble
            );

            yield return null;
        }


        // Idealne 180°
        SetWorldAngle(180f);


        // ===========================================
        // 12–17 SEKUND
        // ŚWIAT 180 → 360
        // ===========================================

        yield return RotateWorld(
            180f,
            360f,
            returnDuration
        );


        // Świat znów jest normalnie.
        RestoreWorld();


        // ===========================================
        // 17–20 SEKUND
        // GRACZ WRACA
        // ===========================================

        Vector3 returnStartPosition =
            xrOrigin.position;

        yield return MovePlayer(
            returnStartPosition,
            originalXROriginPosition,
            playerReturnDuration
        );


        // ===========================================
        // KONIEC
        // ===========================================

        RestorePhysicsAndPlayerControls();

        effectRoutine = null;
    }


    // ==================================================
    // PREPARE PLAYER
    // ==================================================

    void PreparePlayer()
    {
        originalXROriginPosition =
            xrOrigin.position;


        // Wyłącz normalne locomotion.
        if (locomotionRoot)
        {
            locomotionWasActive =
                locomotionRoot.activeSelf;

            locomotionRoot.SetActive(false);
        }


        // Jeżeli XR Origin posiada CharacterController,
        // wyłączamy go na czas magicznego ruchu.
        characterController =
            xrOrigin.GetComponent<CharacterController>();

        if (characterController)
        {
            characterControllerWasEnabled =
                characterController.enabled;

            characterController.enabled = false;
        }


        playerStatePrepared = true;
    }


    // ==================================================
    // CALCULATE XR POSITION
    // ==================================================

    Vector3 CalculateXRPositionForHMDTarget(
        Vector3 desiredHMDPosition
    )
    {
        // Nie możemy po prostu zrobić:
        //
        // xrOrigin.position = playerCenterPoint.position
        //
        // ponieważ Main Camera ma własny offset
        // wynikający z trackingu headsetu.
        //
        // Liczymy więc, o ile trzeba przesunąć
        // cały XR Origin, żeby HMD znalazł się
        // dokładnie w naszym punkcie.

        Vector3 difference =
            desiredHMDPosition -
            hmd.position;

        return
            xrOrigin.position +
            difference;
    }


    // ==================================================
    // MOVE PLAYER
    // ==================================================

    IEnumerator MovePlayer(
        Vector3 startPosition,
        Vector3 targetPosition,
        float duration
    )
    {
        if (duration <= 0f)
        {
            xrOrigin.position =
                targetPosition;

            yield break;
        }


        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            float smoothT =
                movementCurve.Evaluate(t);


            xrOrigin.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    smoothT
                );

            yield return null;
        }


        xrOrigin.position =
            targetPosition;
    }


    // ==================================================
    // PREPARE WORLD PIVOT
    // ==================================================

    void PrepareWorldPivot()
    {
        EnsurePivot();


        // Zapamiętujemy oryginalny WorldRoot.
        originalWorldParent =
            worldRoot.parent;

        originalWorldSiblingIndex =
            worldRoot.GetSiblingIndex();

        originalWorldLocalPosition =
            worldRoot.localPosition;

        originalWorldLocalRotation =
            worldRoot.localRotation;

        originalWorldLocalScale =
            worldRoot.localScale;


        // Pivot umieszczamy dokładnie
        // w miejscu głowy gracza.
        rotationPivot.position =
            hmd.position;

        rotationPivot.rotation =
            Quaternion.identity;

        rotationPivot.localScale =
            Vector3.one;


        // Obracamy świat wokół poziomej osi
        // zgodnej z kierunkiem patrzenia gracza.
        rotationAxis =
            Vector3.ProjectOnPlane(
                hmd.forward,
                Vector3.up
            );


        if (
            rotationAxis.sqrMagnitude <
            0.001f
        )
        {
            rotationAxis =
                Vector3.forward;
        }


        rotationAxis.Normalize();


        // Przepinamy świat pod pivot,
        // zachowując jego aktualne położenie.
        worldRoot.SetParent(
            rotationPivot,
            true
        );


        worldAttachedToPivot = true;
    }


    // ==================================================
    // ROTATE WORLD
    // ==================================================

    IEnumerator RotateWorld(
        float startAngle,
        float endAngle,
        float duration
    )
    {
        if (duration <= 0f)
        {
            SetWorldAngle(endAngle);

            yield break;
        }


        float timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            float smoothT =
                movementCurve.Evaluate(t);


            float angle =
                Mathf.Lerp(
                    startAngle,
                    endAngle,
                    smoothT
                );


            SetWorldAngle(angle);

            yield return null;
        }


        SetWorldAngle(endAngle);
    }


    void SetWorldAngle(float angle)
    {
        if (!rotationPivot)
            return;


        rotationPivot.rotation =
            Quaternion.AngleAxis(
                angle,
                rotationAxis
            );
    }


    // ==================================================
    // PIVOT
    // ==================================================

    void EnsurePivot()
    {
        if (rotationPivot)
            return;


        GameObject pivotObject =
            new GameObject(
                "UpsideDownWorld_RuntimePivot"
            );


        rotationPivot =
            pivotObject.transform;
    }


    // ==================================================
    // RIGIDBODIES
    // ==================================================

    void PrepareRigidbodies()
    {
        rigidbodyStates.Clear();


        Rigidbody[] bodies =
            worldRoot.GetComponentsInChildren<Rigidbody>(
                true
            );


        foreach (Rigidbody rb in bodies)
        {
            if (!rb)
                continue;


            RigidbodyState state =
                new RigidbodyState();


            state.rb =
                rb;

            state.wasKinematic =
                rb.isKinematic;

            state.usedGravity =
                rb.useGravity;


            rigidbodyStates.Add(state);


            rb.useGravity = false;
            rb.isKinematic = true;
        }
    }


    // ==================================================
    // RESTORE WORLD
    // ==================================================

    void RestoreWorld()
    {
        if (!worldAttachedToPivot)
            return;


        worldRoot.SetParent(
            originalWorldParent,
            false
        );


        worldRoot.localPosition =
            originalWorldLocalPosition;

        worldRoot.localRotation =
            originalWorldLocalRotation;

        worldRoot.localScale =
            originalWorldLocalScale;


        worldRoot.SetSiblingIndex(
            originalWorldSiblingIndex
        );


        if (rotationPivot)
        {
            rotationPivot.rotation =
                Quaternion.identity;
        }


        worldAttachedToPivot = false;
    }


    // ==================================================
    // RESTORE PHYSICS + PLAYER
    // ==================================================

    void RestorePhysicsAndPlayerControls()
    {
        foreach (
            RigidbodyState state
            in rigidbodyStates
        )
        {
            if (!state.rb)
                continue;


            state.rb.isKinematic =
                state.wasKinematic;

            state.rb.useGravity =
                state.usedGravity;
        }


        rigidbodyStates.Clear();


        if (
            characterController &&
            characterControllerWasEnabled
        )
        {
            characterController.enabled =
                true;
        }


        if (
            locomotionRoot &&
            locomotionWasActive
        )
        {
            locomotionRoot.SetActive(
                true
            );
        }


        playerStatePrepared = false;
    }


    // ==================================================
    // EMERGENCY RESTORE
    // ==================================================

    void RestoreImmediately()
    {
        RestoreWorld();


        if (
            playerStatePrepared &&
            xrOrigin
        )
        {
            xrOrigin.position =
                originalXROriginPosition;
        }


        RestorePhysicsAndPlayerControls();
    }


    void OnDisable()
    {
        if (effectRoutine != null)
        {
            StopCoroutine(effectRoutine);

            effectRoutine = null;
        }


        RestoreImmediately();
    }


    void OnDestroy()
    {
        RestoreImmediately();


        if (rotationPivot)
        {
            Destroy(
                rotationPivot.gameObject
            );
        }
    }
}