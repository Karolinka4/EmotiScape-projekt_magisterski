using UnityEngine;
using Unity.VRTemplate;

public class FaucetController : MonoBehaviour
{
    public ParticleSystem faucetParticles;
    public XRKnob faucetKnob;

    [Range(0f, 1f)]
    public float activationThreshold = 0.5f;

    public float refillRate = 0.2f;

    private bool isFaucetOn = false;
    private WaterPour canInZone;

    void Start()
    {
        if (faucetParticles != null)
        {
            faucetParticles.Stop();
            faucetParticles.Clear();
        }
    }

    void Update()
    {
        if (faucetKnob == null) return;

        Debug.Log("Knob value: " + faucetKnob.value);

        bool shouldBeOn = faucetKnob.value >= activationThreshold;

        if (shouldBeOn != isFaucetOn)
            SetFaucetState(shouldBeOn);

        if (isFaucetOn && canInZone != null)
            canInZone.Refill(refillRate * Time.deltaTime);
    }

    void SetFaucetState(bool state)
    {
        isFaucetOn = state;

        if (faucetParticles == null) return;

        if (isFaucetOn)
            faucetParticles.Play();
        else
        {
            faucetParticles.Stop();
            faucetParticles.Clear();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        WaterPour waterCan = other.GetComponentInParent<WaterPour>();

        if (waterCan != null)
            canInZone = waterCan;
    }

    void OnTriggerExit(Collider other)
    {
        WaterPour waterCan = other.GetComponentInParent<WaterPour>();

        if (waterCan != null && waterCan == canInZone)
            canInZone = null;
    }
}