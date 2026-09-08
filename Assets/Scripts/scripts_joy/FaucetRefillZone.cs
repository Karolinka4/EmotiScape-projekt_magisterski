using UnityEngine;

public class FaucetRefillZone : MonoBehaviour
{
    public SimpleFaucetKnob faucetKnob;

    void OnTriggerEnter(Collider other)
    {
        WaterPour waterCan = other.GetComponentInParent<WaterPour>();

        if (waterCan != null && faucetKnob != null)
        {
            faucetKnob.SetCanInZone(waterCan);
        }
    }

    void OnTriggerExit(Collider other)
    {
        WaterPour waterCan = other.GetComponentInParent<WaterPour>();

        if (waterCan != null && faucetKnob != null)
        {
            faucetKnob.ClearCanInZone(waterCan);
        }
    }
}