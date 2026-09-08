using UnityEngine;

[ExecuteAlways]
public class PassPositionToMaterial : MonoBehaviour
{
    [Header("Przeci¹gnij TU obiekt Plane (wodospad), który ma materia³")]
    public Renderer waterfallRenderer;

    private MaterialPropertyBlock mpb;

    private static readonly int SpherePosId = Shader.PropertyToID("_SpherePosition");
    private static readonly int SphereRadId = Shader.PropertyToID("_SphereRadius");

    void OnEnable()
    {
        CacheMaterial();
    }

    void CacheMaterial()
    {
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }
    }

    void Update()
    {
        if (waterfallRenderer == null) return;
        if (mpb == null)
        {
            mpb = new MaterialPropertyBlock();
        }

        waterfallRenderer.GetPropertyBlock(mpb);
        mpb.SetVector(SpherePosId, transform.position);
        mpb.SetFloat(SphereRadId, transform.localScale.x);
        waterfallRenderer.SetPropertyBlock(mpb);
    }
}