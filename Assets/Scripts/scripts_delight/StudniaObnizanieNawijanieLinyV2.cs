using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public enum StarowyStanStudni2
{
    gora,
    dol
}

[Serializable]
public class FloatUnityEvent : UnityEvent<float> { }

public class StudniaObnizanieNawijanieLinyV2 : MonoBehaviour
{
    [Header("Referencje (wymagane)")]
    public Transform pivotKolba;
    public Transform linaWiszaca;
    public Transform segmentyParent;

    [Header("Parametry liny")]
    public float metryNaObrot = 300f;
    public float minDlugosc = 90f;
    public float maxDlugosc = 500f;

    [Header("Klawiatura")]
    public float angularAccel = 360f;
    public float angularDamping = 6f;
    public float maxAngularVelocity = 720f;
    public bool allowKeyboardTest = true;

    [Header("VR")]
    public bool useVrGrab = true;
    public Transform vrHandTransform;
    public bool invertVrDirection = false;
    public float vrDeadzoneDegPerFrame = 0.005f;
    public float vrRotationMultiplier = 0.3f;
    public float maxVrDeltaPerFrame = 0.3f;
    public float minHandRadius = 0.10f;
    public int ignoreFramesAfterGrab = 2;

    [Header("Opór przy koñcach")]
    public float resistanceDistance = 25f;
    public float resistanceMax = 0.85f;

    [Header("Segmenty nawiniêcia (wizual)")]
    public float segmentSmoothTime = 0.08f;
    public bool sortSegmentyPoHierarchii = true;

    [Header("Stan pocz¹tkowy")]
    public StarowyStanStudni2 startStudni = StarowyStanStudni2.gora;

    [Header("Audio (opcjonalne)")]
    public AudioSource ropeAudio;
    public float audioMinSpeed = 5f;
    public float audioMaxSpeed = 240f;
    public float audioMinPitch = 0.9f;
    public float audioMaxPitch = 1.3f;
    public float audioMaxVolume = 0.8f;

    [Header("Eventy (opcjonalne)")]
    public UnityEvent OnReachedTop;
    public UnityEvent OnReachedBottom;
    public FloatUnityEvent OnLengthChanged;

    private MeshRenderer[] segmenty;
    private int aktualneSegmenty = -1;

    private float aktualnaDlugosc;
    private float angularVelocity;

    private bool isGrabbed;
    private float lastHandAngle;
    private float tSmooth;
    private float tSmoothVel;

    private bool wasAtTop;
    private bool wasAtBottom;
    private int framesToIgnoreAfterGrab;

    private void Awake()
    {
        if (segmentyParent != null)
        {
            var all = segmentyParent.GetComponentsInChildren<MeshRenderer>(true);
            segmenty = sortSegmentyPoHierarchii
                ? all.OrderBy(r => r.transform.GetSiblingIndex()).ToArray()
                : all.OrderBy(r => r.gameObject.name).ToArray();
        }
        else
        {
            segmenty = Array.Empty<MeshRenderer>();
        }
    }

    private void Start()
    {
        aktualnaDlugosc = (startStudni == StarowyStanStudni2.gora) ? minDlugosc : maxDlugosc;
        UpdateVisuals(true);
        UpdateLimitEvents(true);
    }

    private void Update()
    {
        if (useVrGrab && isGrabbed && vrHandTransform != null)
        {
            UpdateVrRotation();
        }
        else
        {
            UpdateKeyboardRotation();
        }

        UpdateVisuals(false);
        UpdateAudio();
        UpdateLimitEvents(false);
    }

    private void UpdateVrRotation()
    {
        if (framesToIgnoreAfterGrab > 0)
        {
            framesToIgnoreAfterGrab--;
            lastHandAngle = GetHandAngle();
            angularVelocity = 0f;
            return;
        }

        float currentAngle = GetHandAngle();
        float deltaAngle = Mathf.DeltaAngle(lastHandAngle, currentAngle);
        lastHandAngle = currentAngle;

        if (invertVrDirection)
            deltaAngle = -deltaAngle;

        if (Mathf.Abs(deltaAngle) < vrDeadzoneDegPerFrame)
        {
            angularVelocity = 0f;
            return;
        }

        deltaAngle = Mathf.Clamp(deltaAngle, -maxVrDeltaPerFrame, maxVrDeltaPerFrame);
        deltaAngle *= vrRotationMultiplier;

        ApplyDirectRotation(deltaAngle);

        angularVelocity = deltaAngle / Mathf.Max(Time.deltaTime, 0.0001f);
    }

    private void UpdateKeyboardRotation()
    {
        float inputAxis = 0f;

        if (allowKeyboardTest)
        {
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.eKey.isPressed) inputAxis += 1f;
                if (kb.qKey.isPressed) inputAxis -= 1f;
            }

            if (!Mathf.Approximately(inputAxis, 0f))
            {
                float accel = inputAxis * angularAccel;
                ApplyAngularAcceleration(accel);
            }
        }

        ApplyDamping();
        angularVelocity = Mathf.Clamp(angularVelocity, -maxAngularVelocity, maxAngularVelocity);

        float deltaRot = angularVelocity * Time.deltaTime;
        if (!Mathf.Approximately(deltaRot, 0f))
            ApplyDirectRotation(deltaRot);
    }

    public void SetHandTransform(Transform hand)
    {
        vrHandTransform = hand;
    }

    public void SetGrabbed(bool grabbed)
    {
        isGrabbed = grabbed;

        if (grabbed && vrHandTransform != null)
        {
            lastHandAngle = GetHandAngle();
            angularVelocity = 0f;
            framesToIgnoreAfterGrab = ignoreFramesAfterGrab;
        }
        else
        {
            angularVelocity = 0f;
            framesToIgnoreAfterGrab = 0;
        }
    }

    private void ApplyAngularAcceleration(float accel)
    {
        float resistance = ComputeResistance01();
        float scale = 1f - (resistance * resistanceMax);
        angularVelocity += accel * scale * Time.deltaTime;
    }

    private void ApplyDamping()
    {
        float damp = Mathf.Exp(-angularDamping * Time.deltaTime);
        angularVelocity *= damp;
    }

    private void ApplyDirectRotation(float deltaRot)
    {
        if (pivotKolba == null) return;

        float deltaLiny = (deltaRot / 360f) * metryNaObrot;
        float nowaDlugosc = aktualnaDlugosc + deltaLiny;

        float clamped = Mathf.Clamp(nowaDlugosc, minDlugosc, maxDlugosc);

        if (!Mathf.Approximately(clamped, nowaDlugosc))
        {
            float realDeltaLiny = clamped - aktualnaDlugosc;
            float realDeltaRot = (realDeltaLiny / metryNaObrot) * 360f;

            deltaRot = realDeltaRot;
            nowaDlugosc = clamped;
            angularVelocity = 0f;
        }

        float resistance = ComputeResistance01();
        float scale = 1f - (resistance * resistanceMax);
        deltaRot *= scale;

        // Zostaw Vector3.up, skoro pisa³aœ ¿e na tej osi jest prawie dobrze
        pivotKolba.Rotate(Vector3.up, deltaRot, Space.Self);

        aktualnaDlugosc = nowaDlugosc;
        OnLengthChanged?.Invoke(aktualnaDlugosc);
    }

    private float ComputeResistance01()
    {
        float distToMin = Mathf.Abs(aktualnaDlugosc - minDlugosc);
        float distToMax = Mathf.Abs(maxDlugosc - aktualnaDlugosc);
        float dist = Mathf.Min(distToMin, distToMax);

        if (resistanceDistance <= 0.001f) return 0f;

        float proximity = 1f - Mathf.Clamp01(dist / resistanceDistance);
        return Mathf.SmoothStep(0f, 1f, proximity);
    }

    private float GetHandAngle()
    {
        if (pivotKolba == null || vrHandTransform == null)
            return 0f;

        Vector3 axis = pivotKolba.TransformDirection(Vector3.up).normalized;

        Vector3 toHand = vrHandTransform.position - pivotKolba.position;
        Vector3 projected = Vector3.ProjectOnPlane(toHand, axis);

        float radius = projected.magnitude;
        if (radius < minHandRadius)
            return lastHandAngle;

        projected /= radius;

        Vector3 refDir = Vector3.ProjectOnPlane(pivotKolba.TransformDirection(Vector3.forward), axis);
        if (refDir.sqrMagnitude < 0.0001f)
            refDir = Vector3.ProjectOnPlane(pivotKolba.TransformDirection(Vector3.right), axis);

        if (refDir.sqrMagnitude < 0.0001f)
            return lastHandAngle;

        refDir.Normalize();

        return Vector3.SignedAngle(refDir, projected, axis);
    }

    private void UpdateVisuals(bool force)
    {
        if (linaWiszaca != null)
        {
            var s = linaWiszaca.localScale;
            linaWiszaca.localScale = new Vector3(s.x, s.y, aktualnaDlugosc);
        }

        if (segmenty == null || segmenty.Length == 0) return;

        float t = Mathf.InverseLerp(maxDlugosc, minDlugosc, aktualnaDlugosc);
        tSmooth = Mathf.SmoothDamp(tSmooth, t, ref tSmoothVel, segmentSmoothTime);

        int docelowe = Mathf.FloorToInt(tSmooth * segmenty.Length + 0.0001f);
        docelowe = Mathf.Clamp(docelowe, 0, segmenty.Length);

        if (!force && docelowe == aktualneSegmenty) return;
        aktualneSegmenty = docelowe;

        for (int i = 0; i < segmenty.Length; i++)
            segmenty[i].enabled = (i < aktualneSegmenty);
    }

    private void UpdateLimitEvents(bool force)
    {
        bool atTop = Mathf.Approximately(aktualnaDlugosc, minDlugosc);
        bool atBottom = Mathf.Approximately(aktualnaDlugosc, maxDlugosc);

        if (force)
        {
            wasAtTop = atTop;
            wasAtBottom = atBottom;
            return;
        }

        if (atTop && !wasAtTop) OnReachedTop?.Invoke();
        if (atBottom && !wasAtBottom) OnReachedBottom?.Invoke();

        wasAtTop = atTop;
        wasAtBottom = atBottom;
    }

    private void UpdateAudio()
    {
        if (ropeAudio == null) return;

        float speed = Mathf.Abs(angularVelocity);

        if (speed < audioMinSpeed)
        {
            if (ropeAudio.isPlaying) ropeAudio.Stop();
            return;
        }

        if (!ropeAudio.isPlaying) ropeAudio.Play();

        float t = Mathf.InverseLerp(audioMinSpeed, audioMaxSpeed, speed);
        ropeAudio.pitch = Mathf.Lerp(audioMinPitch, audioMaxPitch, t);
        ropeAudio.volume = Mathf.Lerp(0f, audioMaxVolume, t);
    }

    public void OnSelectedEntered(SelectEnterEventArgs args)
    {
        useVrGrab = true;
        SetHandTransform(args.interactorObject.transform);
        SetGrabbed(true);
    }

    public void OnSelectedExited(SelectExitEventArgs args)
    {
        SetGrabbed(false);
    }
}