using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class HighlightOnHover : MonoBehaviour
{
    [Tooltip("Wspólny materia³ obwódki (np. bia³y z Emission)")]
    public Material outlineMaterial;

    private XRGrabInteractable grabInteractable;
    private MeshRenderer meshRenderer;
    private Material[] originalMaterials;

    void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        meshRenderer = GetComponent<MeshRenderer>();

        // Zapisujemy oryginalne materia³y diamentu (np. ró¿owy, niebieski)
        if (meshRenderer != null)
        {
            originalMaterials = meshRenderer.sharedMaterials;
        }

        grabInteractable.hoverEntered.AddListener(OnHoverEnter);
        grabInteractable.hoverExited.AddListener(OnHoverExit);
    }

    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (meshRenderer == null || outlineMaterial == null) return;

        // Tworzymy now¹ listê: oryginalne materia³y + materia³ obwódki na wierzch
        List<Material> mats = new List<Material>(originalMaterials);
        mats.Add(outlineMaterial);
        meshRenderer.materials = mats.ToArray();
    }

    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (meshRenderer == null) return;

        // Przywracamy dok³adnie te materia³y, które by³y na pocz¹tku
        meshRenderer.materials = originalMaterials;
    }

    void OnDestroy()
    {
        if (grabInteractable != null)
        {
            grabInteractable.hoverEntered.RemoveListener(OnHoverEnter);
            grabInteractable.hoverExited.RemoveListener(OnHoverExit);
        }
    }
}