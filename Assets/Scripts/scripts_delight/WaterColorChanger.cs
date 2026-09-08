using System.Collections.Generic;
using UnityEngine;

public class WaterColorChanger : MonoBehaviour
{
    [Header("Renderer wody")]
    public Renderer waterRenderer;

    [Header("Nazwa pola koloru w materiale")]
    public string colorPropertyName = "_BaseColor";

    [Header("Kolor startowy wody")]
    public Color defaultWaterColor = Color.blue;

    private Material waterMaterial;
    private readonly List<DiamondWaterColor> diamondsInside = new List<DiamondWaterColor>();

    private void Awake()
    {
        if (waterRenderer == null)
            waterRenderer = GetComponent<Renderer>();

        if (waterRenderer != null)
        {
            waterMaterial = waterRenderer.material;
            waterMaterial.SetColor(colorPropertyName, defaultWaterColor);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        DiamondWaterColor diamond = other.GetComponent<DiamondWaterColor>();
        if (diamond == null) return;

        if (!diamondsInside.Contains(diamond))
            diamondsInside.Add(diamond);

        UpdateWaterColor();
    }

    private void OnTriggerExit(Collider other)
    {
        DiamondWaterColor diamond = other.GetComponent<DiamondWaterColor>();
        if (diamond == null) return;

        if (diamondsInside.Contains(diamond))
            diamondsInside.Remove(diamond);

        UpdateWaterColor();
    }

    private void UpdateWaterColor()
    {
        if (waterMaterial == null) return;

        if (diamondsInside.Count == 0)
        {
            waterMaterial.SetColor(colorPropertyName, defaultWaterColor);
            return;
        }

        DiamondWaterColor lastDiamond = diamondsInside[diamondsInside.Count - 1];
        waterMaterial.SetColor(colorPropertyName, lastDiamond.waterColor);
    }
}