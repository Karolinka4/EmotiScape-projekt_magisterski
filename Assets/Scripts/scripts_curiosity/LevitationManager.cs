using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevitationManager : MonoBehaviour
{
    [Header("Objects")]
    public List<Transform> objectsToLevitate = new List<Transform>();

    [Header("Phase Times")]
    public float shakeDuration = 2f;
    public float riseDuration = 3f;
    public float hoverDuration = 12f;
    public float fallDuration = 3f;

    [Header("Shake")]
    public float shakeAmount = 0.025f;

    [Header("Rise")]
    public float minHeight = 0.5f;
    public float maxHeight = 1.5f;

    [Header("Hover")]
    public float bobAmount = 0.08f;
    public float bobSpeed = 2f;

    [Header("Rotation")]
    public float minRotationSpeed = 10f;
    public float maxRotationSpeed = 25f;

    Coroutine levitationRoutine;

    class ObjectState
    {
        public Transform target;

        public Vector3 startPosition;
        public Quaternion startRotation;

        public float height;
        public float bobOffset;

        public Vector3 rotationAxis;
        public float rotationSpeed;

        public Rigidbody rb;
        public bool originalIsKinematic;
        public bool originalUseGravity;

        public Vector3 fallStartPosition;
        public Quaternion fallStartRotation;
    }

    List<ObjectState> states = new List<ObjectState>();


    public void StartLevitation()
    {
        // Je¿eli mikstura zostanie wypita ponownie
        // podczas dzia³ania efektu:
        // najpierw przywracamy wszystko i zaczynamy od nowa.
        if (levitationRoutine != null)
        {
            StopCoroutine(levitationRoutine);
            RestoreObjects();
        }

        PrepareObjects();

        levitationRoutine =
            StartCoroutine(LevitationRoutine());
    }


    void PrepareObjects()
    {
        states.Clear();

        foreach (Transform obj in objectsToLevitate)
        {
            if (!obj)
                continue;

            ObjectState state = new ObjectState();

            state.target = obj;

            // Zapamiêtujemy dok³adne miejsce i obrót
            state.startPosition = obj.position;
            state.startRotation = obj.rotation;

            // Ka¿dy przedmiot uniesie siê trochê inaczej
            state.height =
                Random.Range(minHeight, maxHeight);

            // Dziêki temu przedmioty nie ko³ysz¹ siê identycznie
            state.bobOffset =
                Random.Range(0f, Mathf.PI * 2f);

            state.rotationAxis =
                Random.onUnitSphere.normalized;

            state.rotationSpeed =
                Random.Range(
                    minRotationSpeed,
                    maxRotationSpeed
                );

            // Je¿eli przedmiot posiada Rigidbody
            state.rb = obj.GetComponent<Rigidbody>();

            if (state.rb)
            {
                state.originalIsKinematic =
                    state.rb.isKinematic;

                state.originalUseGravity =
                    state.rb.useGravity;

                // Na czas efektu fizyka nie steruje obiektem
                state.rb.useGravity = false;
                state.rb.isKinematic = true;
            }

            states.Add(state);
        }
    }


    IEnumerator LevitationRoutine()
    {
        // =========================
        // 0–2 SEKUND
        // DR¯ENIE
        // =========================

        float timer = 0f;

        while (timer < shakeDuration)
        {
            timer += Time.deltaTime;

            foreach (ObjectState state in states)
            {
                if (!state.target)
                    continue;

                Vector3 shake =
                    Random.insideUnitSphere *
                    shakeAmount;

                state.target.position =
                    state.startPosition + shake;

                state.target.rotation =
                    state.startRotation;
            }

            yield return null;
        }


        // Przywróæ dok³adn¹ pozycjê
        // przed rozpoczêciem unoszenia.
        foreach (ObjectState state in states)
        {
            if (!state.target)
                continue;

            state.target.position =
                state.startPosition;
        }


        // =========================
        // 2–5 SEKUND
        // UNOSZENIE
        // =========================

        timer = 0f;

        while (timer < riseDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(timer / riseDuration);

            // ³agodne rozpoczêcie i zakoñczenie ruchu
            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            foreach (ObjectState state in states)
            {
                if (!state.target)
                    continue;

                Vector3 targetPosition =
                    state.startPosition +
                    Vector3.up * state.height;

                state.target.position =
                    Vector3.Lerp(
                        state.startPosition,
                        targetPosition,
                        smoothT
                    );

                float rotationAmount =
                    state.rotationSpeed * timer;

                state.target.rotation =
                    Quaternion.AngleAxis(
                        rotationAmount,
                        state.rotationAxis
                    )
                    * state.startRotation;
            }

            yield return null;
        }


        // =========================
        // 5–17 SEKUND
        // LEWITACJA
        // =========================

        timer = 0f;

        while (timer < hoverDuration)
        {
            timer += Time.deltaTime;

            foreach (ObjectState state in states)
            {
                if (!state.target)
                    continue;

                Vector3 basePosition =
                    state.startPosition +
                    Vector3.up * state.height;

                float bob =
                    Mathf.Sin(
                        timer * bobSpeed +
                        state.bobOffset
                    )
                    * bobAmount;

                state.target.position =
                    basePosition +
                    Vector3.up * bob;

                // Kontynuujemy rotacjê
                // rozpoczêt¹ podczas unoszenia
                float totalRotationTime =
                    riseDuration + timer;

                float rotationAmount =
                    state.rotationSpeed *
                    totalRotationTime;

                state.target.rotation =
                    Quaternion.AngleAxis(
                        rotationAmount,
                        state.rotationAxis
                    )
                    * state.startRotation;
            }

            yield return null;
        }


        // Zapamiêtujemy dok³adne miejsce,
        // w którym ka¿dy przedmiot koñczy lewitowanie.
        foreach (ObjectState state in states)
        {
            if (!state.target)
                continue;

            state.fallStartPosition =
                state.target.position;

            state.fallStartRotation =
                state.target.rotation;
        }


        // =========================
        // 17–20 SEKUND
        // POWRÓT
        // =========================

        timer = 0f;

        while (timer < fallDuration)
        {
            timer += Time.deltaTime;

            float t =
                Mathf.Clamp01(timer / fallDuration);

            float smoothT =
                Mathf.SmoothStep(0f, 1f, t);

            foreach (ObjectState state in states)
            {
                if (!state.target)
                    continue;

                state.target.position =
                    Vector3.Lerp(
                        state.fallStartPosition,
                        state.startPosition,
                        smoothT
                    );

                state.target.rotation =
                    Quaternion.Slerp(
                        state.fallStartRotation,
                        state.startRotation,
                        smoothT
                    );
            }

            yield return null;
        }


        // =========================
        // KONIEC
        // =========================

        RestoreObjects();

        levitationRoutine = null;
    }


    void RestoreObjects()
    {
        foreach (ObjectState state in states)
        {
            if (!state.target)
                continue;

            // Dok³adny powrót
            state.target.position =
                state.startPosition;

            state.target.rotation =
                state.startRotation;

            // Przywracamy fizykê tak¹,
            // jaka by³a przed mikstur¹.
            if (state.rb)
            {
                state.rb.isKinematic =
                    state.originalIsKinematic;

                state.rb.useGravity =
                    state.originalUseGravity;
            }
        }

        states.Clear();
    }


    void OnDisable()
    {
        // Zabezpieczenie:
        // jeœli obiekt ze skryptem zostanie wy³¹czony
        // podczas efektu, wszystko wraca na miejsce.
        if (levitationRoutine != null)
        {
            StopCoroutine(levitationRoutine);
            levitationRoutine = null;
        }

        RestoreObjects();
    }
}