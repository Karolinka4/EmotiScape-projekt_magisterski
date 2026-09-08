using UnityEngine;

public class BottleLiquid : MonoBehaviour
{
    [Header("Refs")]
    public Transform liquidVisual;
    public Renderer liquidRenderer;
    public Transform mouthPoint;
    public Transform dipPoint;

    [Header("Player Effects")]
    public PlayerPotionEffects playerEffects;

    [Header("Liquid State")]
    [Range(0, 1)]
    public float fill01 = 0f;

    public Color liquidColor = Color.clear;

    // Jaki efekt znajduje siê aktualnie w butelce
    public PotionEffectType StoredPotionEffect { get; private set; }
        = PotionEffectType.None;

    [Header("Visual Tuning")]
    public float minLiquidScaleY = 0.001f;
    public float maxLiquidScaleY = 1.0f;
    public Vector3 liquidLocalPosEmpty;
    public Vector3 liquidLocalPosFull;

    [Header("Drinking")]
    public Transform hmd;

    public float drinkDistance = 0.2f;
    public float minTiltAngle = 60f;
    public float drinkRatePerSec = 0.25f;

    MaterialPropertyBlock mpb;

    bool dirtyVisual;
    bool isFilling;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
        dirtyVisual = true;
    }

    void Update()
    {
        HandleDrinking();
    }

    void LateUpdate()
    {
        if (dirtyVisual)
        {
            ApplyVisual();
            dirtyVisual = false;
        }
    }

    // Wo³ane przez kocio³
    public void BeginFilling(
        Color color,
        PotionEffectType effect
    )
    {
        isFilling = true;

        // Je¿eli butelka by³a pusta,
        // zapisujemy miksturê z kot³a.
        if (fill01 <= 0.001f)
        {
            liquidColor = color;
            StoredPotionEffect = effect;
        }

        dirtyVisual = true;
    }

    public void EndFilling()
    {
        isFilling = false;
    }

    public void AddFill(
        float amount,
        float maxAllowed01 = 1f
    )
    {
        if (amount <= 0f)
            return;

        float newFill = fill01 + amount;

        if (newFill > maxAllowed01)
            newFill = maxAllowed01;

        if (!Mathf.Approximately(newFill, fill01))
        {
            fill01 = newFill;
            dirtyVisual = true;
        }
    }

    void HandleDrinking()
    {
        if (fill01 <= 0f)
            return;

        if (!hmd)
            return;

        Transform p =
            mouthPoint ? mouthPoint : transform;

        float maxDistSqr =
            drinkDistance * drinkDistance;

        Vector3 d =
            p.position - hmd.position;

        // Butelka za daleko od g³owy
        if (d.sqrMagnitude > maxDistSqr)
            return;

        float angleFromUp =
            Vector3.Angle(
                transform.up,
                Vector3.up
            );

        // Butelka za ma³o przechylona
        if (angleFromUp < minTiltAngle)
            return;

        float previousFill = fill01;

        fill01 -=
            drinkRatePerSec * Time.deltaTime;

        if (fill01 < 0f)
            fill01 = 0f;

        if (fill01 < previousFill)
        {
            dirtyVisual = true;
        }

        // NAJWA¯NIEJSZY MOMENT:
        //
        // wczeœniej coœ by³o w butelce
        // i w³aœnie wypito ostatni¹ porcjê.
        if (previousFill > 0f && fill01 <= 0f)
        {
            FinishDrinkingPotion();
        }
    }

    void FinishDrinkingPotion()
    {
        // Najpierw zapamiêtujemy efekt,
        // bo zaraz czyœcimy butelkê.
        PotionEffectType effect =
            StoredPotionEffect;

        StoredPotionEffect =
            PotionEffectType.None;

        liquidColor = Color.clear;

        dirtyVisual = true;

        // Zwyk³a woda / z³a mikstura
        if (effect == PotionEffectType.None)
            return;

        if (!playerEffects)
        {
            Debug.LogWarning(
                "BottleLiquid: nie przypisano PlayerPotionEffects."
            );

            return;
        }

        playerEffects.ApplyPotion(effect);
    }

    void ApplyVisual()
    {
        if (!liquidRenderer)
            return;

        liquidRenderer.GetPropertyBlock(mpb);

        mpb.SetColor(
            "_Color",
            liquidColor
        );

        mpb.SetColor(
            "_BaseColor",
            liquidColor
        );

        liquidRenderer.SetPropertyBlock(mpb);

        liquidRenderer.enabled =
            fill01 > 0.001f;
    }
}