using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class SimpleFaucetKnob : MonoBehaviour
{
    public ParticleSystem faucetParticles;

    [Header("Obrót korka")]
    public Transform knobVisual;
    public float maxAngle = 90f;
    public float turnSpeed = 120f;

    [Header("Nalewanie")]
    public float refillRate = 0.2f;

    private float currentAngle = 0f;
    private bool isHeld = false;
    private bool isOpening = true;
    private bool isFaucetOn = false;
    private WaterPour canInZone;

    void Start()
    {
        if (faucetParticles != null)
        {
            faucetParticles.Stop();
            faucetParticles.Clear();
        }

        UpdateKnobRotation();
    }

    void Update()
    {
        if (isHeld)
        {
            if (isOpening)
                currentAngle += turnSpeed * Time.deltaTime;
            else
                currentAngle -= turnSpeed * Time.deltaTime;

            currentAngle = Mathf.Clamp(currentAngle, 0f, maxAngle);

            UpdateKnobRotation();

            if (currentAngle >= maxAngle)
            {
                currentAngle = maxAngle;
                isHeld = false;
                isOpening = false;
            }
            else if (currentAngle <= 0f)
            {
                currentAngle = 0f;
                isHeld = false;
                isOpening = true;
            }
        }

        isFaucetOn = currentAngle >= maxAngle * 0.5f;

        if (faucetParticles != null)
        {
            if (isFaucetOn && !faucetParticles.isPlaying)
                faucetParticles.Play();

            if (!isFaucetOn && faucetParticles.isPlaying)
            {
                faucetParticles.Stop();
                faucetParticles.Clear();
            }
        }

        if (isFaucetOn && canInZone != null)
            canInZone.Refill(refillRate * Time.deltaTime);
    }

    void UpdateKnobRotation()
    {
        if (knobVisual != null)
            knobVisual.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
    }

    public void StartTurn()
    {
        isHeld = true;
    }

    public void StopTurn()
    {
        isHeld = false;
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
    public void SetCanInZone(WaterPour waterCan)
    {
        canInZone = waterCan;
    }

    public void ClearCanInZone(WaterPour waterCan)
    {
        if (canInZone == waterCan)
            canInZone = null;
    }
}