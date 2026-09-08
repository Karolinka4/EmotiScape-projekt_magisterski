using UnityEngine;

public class DiamondSlot : MonoBehaviour
{
    public Transform slotPoint;
    public string diamondTag = "Diamond";
    public float snapSpeed = 10f; // Opcjonalne: p³ynne przyci¹ganie

    private Transform attachedDiamond;
    private Rigidbody attachedRb;

    // Te zmienne warto wyzerowaæ, jeœli chcesz, by diament idealnie centrowa³ siê w slocie
    private Vector3 localOffsetPosition;
    private Quaternion localOffsetRotation;

    private void OnTriggerEnter(Collider other)
    {
        // Jeœli slot zajêty lub to nie diament - zignoruj
        if (attachedDiamond != null || !other.CompareTag(diamondTag)) return;

        AttachDiamond(other.transform);
    }

    private void LateUpdate()
    {
        if (attachedDiamond == null || slotPoint == null) return;

        // Jeœli coœ innego (np. rêka gracza) zmieni³o isKinematic na false, 
        // oznacza to, ¿e ktoœ próbuje zabraæ diament.
        if (attachedRb != null && !attachedRb.isKinematic)
        {
            DetachDiamond();
            return;
        }

        // Utrzymywanie diamentu w pozycji slotu
        attachedDiamond.position = slotPoint.TransformPoint(localOffsetPosition);
        attachedDiamond.rotation = slotPoint.rotation * localOffsetRotation;
    }

    public void AttachDiamond(Transform diamond)
    {
        attachedDiamond = diamond;
        attachedRb = diamond.GetComponent<Rigidbody>();

        if (attachedRb != null)
        {
            attachedRb.velocity = Vector3.zero;
            attachedRb.angularVelocity = Vector3.zero;
            attachedRb.isKinematic = true; // To "zamra¿a" diament w slocie
            attachedRb.useGravity = false;
        }

        // Jeœli chcesz, ¿eby diament wskakiwa³ IDEALNIE w œrodek slotu,
        // zamiast zapamiêtywaæ offset, ustaw te wartoœci na zero:
        localOffsetPosition = Vector3.zero;
        localOffsetRotation = Quaternion.identity;
    }

    public void DetachDiamond()
    {
        if (attachedDiamond == null) return;

        if (attachedRb != null)
        {
            attachedRb.isKinematic = false;
            attachedRb.useGravity = true;
        }

        attachedDiamond = null;
        attachedRb = null;

        // Opcjonalnie: wy³¹cz kolizje na sekundê, ¿eby diament 
        // natychmiast nie "wskoczy³" z powrotem do tego samego slotu
        StartCoroutine(IgnoreCollisionTemporarily());
    }

    private System.Collections.IEnumerator IgnoreCollisionTemporarily()
    {
        Collider col = GetComponent<Collider>();
        col.enabled = false;
        yield return new WaitForSeconds(1.0f);
        col.enabled = true;
    }
}