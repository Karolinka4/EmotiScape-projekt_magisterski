using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SkyboxSlot : MonoBehaviour
{
    [Header("Skybox bazowy (t³o0)")]
    public Material defaultSkybox;

    // Aktualnie w³o¿ony diament (jeœli jakiœ jest)
    private DiamondSkybox currentDiamond;

    private Collider slotCollider;

    private void Awake()
    {
        slotCollider = GetComponent<Collider>();
        slotCollider.isTrigger = true; // slot dzia³a jako trigger
    }

    private void OnEnable()
    {
        // Upewnij siê, ¿e na starcie jest t³o0
        if (defaultSkybox != null)
        {
            RenderSettings.skybox = defaultSkybox;
            DynamicGI.UpdateEnvironment();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Bierzemy komponent z obiektu lub jego rodzica (na wypadek siatki/kolidera jako childa)
        var diamond = other.GetComponentInParent<DiamondSkybox>() ?? other.GetComponent<DiamondSkybox>();
        if (diamond == null) return;

        // Jeœli slot ju¿ zajêty, ignoruj (albo tu mo¿esz dodaæ logikê „tylko jeden”/odpychanie)
        if (currentDiamond != null) return;

        // Ustaw nowy skybox
        currentDiamond = diamond;
        if (currentDiamond.skyboxMaterial != null)
        {
            RenderSettings.skybox = currentDiamond.skyboxMaterial;
            DynamicGI.UpdateEnvironment();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var diamond = other.GetComponentInParent<DiamondSkybox>() ?? other.GetComponent<DiamondSkybox>();
        if (diamond == null) return;

        // Zmieniamy na t³o0 tylko jeœli wyszed³ ten sam diament, który by³ „w slocie”
        if (diamond == currentDiamond)
        {
            currentDiamond = null;
            if (defaultSkybox != null)
            {
                RenderSettings.skybox = defaultSkybox;
                DynamicGI.UpdateEnvironment();
            }
        }
    }
}
