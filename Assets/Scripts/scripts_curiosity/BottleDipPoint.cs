using UnityEngine;

public class BottleDipPoint : MonoBehaviour
{
    public BottleLiquid bottle;

    void Reset()
    {
        bottle = GetComponentInParent<BottleLiquid>();
    }
}
