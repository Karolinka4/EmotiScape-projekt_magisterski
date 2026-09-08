using UnityEngine;
using Unity.XR.CoreUtils;

public class ElevatorCabinTrigger : MonoBehaviour
{
    public bool PlayerInside { get; private set; }

    public XROrigin Player { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        CharacterController characterController =
            other.GetComponent<CharacterController>();

        if (characterController == null)
            return;

        XROrigin xrOrigin =
            other.GetComponentInParent<XROrigin>();

        if (xrOrigin == null)
            return;

        PlayerInside = true;
        Player = xrOrigin;

        Debug.Log("Gracz wszed³ do windy");
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterController characterController =
            other.GetComponent<CharacterController>();

        if (characterController == null)
            return;

        XROrigin xrOrigin =
            other.GetComponentInParent<XROrigin>();

        if (xrOrigin == null || xrOrigin != Player)
            return;

        PlayerInside = false;
        Player = null;

        Debug.Log("Gracz wyszed³ z windy");
    }
}