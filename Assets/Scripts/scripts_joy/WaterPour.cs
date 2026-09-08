using UnityEngine;

public class WaterPour : MonoBehaviour
{
    public ParticleSystem waterParticles;

    [Header("K¹t lania")]
    public float pourAngle = 45f;

    [Header("Modele wody")]
    public GameObject waterLow;     // Woda1
    public GameObject waterMedium;  // Woda1.001
    public GameObject waterFull;    // Woda1.002

    [Header("Iloœæ wody")]
    public float waterAmount = 3f;
    public float maxWaterAmount = 3f;
    public float pourRate = 0.2f; // 0.2 = jeden poziom po 5 sekundach

    void Start()
    {
        if (waterParticles != null)
        {
            var main = waterParticles.main;
            main.playOnAwake = false;
            waterParticles.Stop();
        }

        waterAmount = Mathf.Clamp(waterAmount, 0f, maxWaterAmount);
        UpdateWaterVisuals();
    }

    void Update()
    {

        if (waterParticles == null) return;

        float angle = Vector3.Angle(Vector3.up, transform.up);
        Debug.Log("Angle: " + angle);
        bool shouldPour = angle < pourAngle && waterAmount > 0f;

        if (shouldPour)
        {
            if (!waterParticles.isPlaying)
                waterParticles.Play();

            waterAmount -= pourRate * Time.deltaTime;
            waterAmount = Mathf.Clamp(waterAmount, 0f, maxWaterAmount);

            UpdateWaterVisuals();

            if (waterAmount <= 0f)
                StopPouring();
        }
        else
        {
            StopPouring();
        }
        
    }
    public void Refill(float amount)
    {
        waterAmount += amount;
        waterAmount = Mathf.Clamp(waterAmount, 0f, maxWaterAmount);
        UpdateWaterVisuals();
    }

    void StopPouring()
    {
        if (waterParticles != null && waterParticles.isPlaying)
        {
            waterParticles.Stop();
            waterParticles.Clear();
        }
    }

    void UpdateWaterVisuals()
    {
        if (waterLow != null)
            waterLow.SetActive(waterAmount > 0f && waterAmount <= 1f);

        if (waterMedium != null)
            waterMedium.SetActive(waterAmount > 1f && waterAmount <= 2f);

        if (waterFull != null)
            waterFull.SetActive(waterAmount > 2f);
    }
}