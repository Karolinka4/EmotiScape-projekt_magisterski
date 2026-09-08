using UnityEngine;

[ExecuteAlways]
public class PassPositionToWaterfall : MonoBehaviour
{
    [Header("Auto: parent Renderer (plane wodospadu)")]
    [SerializeField] private Renderer waterfallRenderer;

    [Header("Shader property names")]
    [SerializeField] private string positionProperty = "_SpherePosition";
    [SerializeField] private string radiusProperty = "_SphereRadius";

    private MaterialPropertyBlock mpb;

    private void OnEnable()
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();

        // Jeœli nie ustawione rêcznie, bierzemy Renderer z parenta
        if (waterfallRenderer == null && transform.parent != null)
            waterfallRenderer = transform.parent.GetComponent<Renderer>();
    }

    private void Update()
    {
        if (waterfallRenderer == null) return;

        waterfallRenderer.GetPropertyBlock(mpb);

        // Pozycja w WORLD (zgodnie z Twoim graph-em na screenie: Position = World)
        mpb.SetVector(positionProperty, transform.position);

        // Promieñ - mo¿esz braæ z localScale.x jak wczeœniej
        mpb.SetFloat(radiusProperty, transform.localScale.x);

        waterfallRenderer.SetPropertyBlock(mpb);
    }
}