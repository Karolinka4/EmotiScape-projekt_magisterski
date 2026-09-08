using UnityEngine;

public class FlowerWatering : MonoBehaviour
{
    [Header("Materia³y")]
    [SerializeField] private Material dryMaterial;   // Materia³ z suchym kwiatem
    [SerializeField] private Material wateredMaterial; // Materia³ z podlanym kwiatem

    [Header("Ustawienia podlewania")]
     private float waterRequired = 0.01f; // Ile wody potrzebuje kwiat

    // Zmieniamy na tablicê, aby przechowaæ renderery wszystkich liœci
    private MeshRenderer[] childRenderers;
    private float currentWaterAmount = 0f;
    private bool isWatered = false;

    void Start()
    {
        // Pobieramy renderery ze wszystkich obiektów-dzieci (liœci)
        childRenderers = GetComponentsInChildren<MeshRenderer>();

        // Na starcie ustawiamy SUCHY materia³ na wszystkich liœciach
        if (childRenderers != null && childRenderers.Length > 0 && dryMaterial != null)
        {
            SetAllMaterials(dryMaterial);
        }
    }

    void OnParticleCollision(GameObject other)
    {
        if (isWatered) return;

        if (other.CompareTag("Water"))
        {
            currentWaterAmount += Time.deltaTime;

            if (currentWaterAmount >= waterRequired)
            {
                WaterFlower();
            }
        }
    }

    void WaterFlower()
    {
        isWatered = true;
        if (childRenderers != null && childRenderers.Length > 0 && wateredMaterial != null)
        {
            // Podmieniamy na MOKRY materia³ na wszystkich liœciach
            SetAllMaterials(wateredMaterial);
            Debug.Log("Wszystkie liœcie zosta³y podlane!");
        }
    }

    // Funkcja iteruje po ka¿dym liœciu i zmienia jego materia³y
    void SetAllMaterials(Material newMaterial)
    {
        foreach (MeshRenderer renderer in childRenderers)
        {
            if (renderer == null) continue;

            int materialCount = renderer.sharedMaterials.Length;
            Material[] newMaterialsArray = new Material[materialCount];

            for (int i = 0; i < materialCount; i++)
            {
                newMaterialsArray[i] = newMaterial;
            }

            renderer.materials = newMaterialsArray;
        }
    }
}