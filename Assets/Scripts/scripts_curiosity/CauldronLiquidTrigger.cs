using UnityEngine;

public class CauldronLiquidTrigger : MonoBehaviour
{
    public CauldronPotionRecipes recipes;

    public float fillRatePerSec = 0.8f;

    [Range(0f, 1f)]
    public float maxFillAllowed = 1f;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<BottleDipPoint>(out var dp))
            return;

        if (!dp.bottle)
            return;

        dp.bottle.BeginFilling(
            recipes.CurrentCauldronColor,
            recipes.CurrentPotionEffect
        );
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<BottleDipPoint>(out var dp))
            return;

        if (!dp.bottle)
            return;

        dp.bottle.EndFilling();
    }

    void OnTriggerStay(Collider other)
    {
        if (!other.TryGetComponent<BottleDipPoint>(out var dp))
            return;

        if (!dp.bottle)
            return;

        if (dp.bottle.fill01 >= maxFillAllowed)
            return;

        float delta =
            fillRatePerSec * Time.fixedDeltaTime;

        dp.bottle.AddFill(
            delta,
            maxFillAllowed
        );
    }
}