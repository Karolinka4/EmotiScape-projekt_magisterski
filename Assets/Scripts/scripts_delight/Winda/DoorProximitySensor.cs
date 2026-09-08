using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class DoorProximitySensor : MonoBehaviour
{
    [SerializeField] private ElevatorController elevator;

    private void OnTriggerEnter(Collider other)
    {
        CharacterController characterController =
            other.GetComponent<CharacterController>();

        if (characterController == null)
            return;

        XROrigin player =
            other.GetComponentInParent<XROrigin>();

        if (player == null)
            return;

        elevator.OpenDoorsFromProximity();
    }

    private void OnTriggerExit(Collider other)
    {
        CharacterController characterController =
            other.GetComponent<CharacterController>();

        if (characterController == null)
            return;

        XROrigin player =
            other.GetComponentInParent<XROrigin>();

        if (player == null)
            return;

        StartCoroutine(TryCloseDoors());
    }

    private IEnumerator TryCloseDoors()
    {
        // Krótko czekamy, ¿eby CabinTrigger zd¹¿y³ wykryæ,
        // ¿e gracz wszed³ do windy.
        yield return new WaitForSeconds(0.2f);

        elevator.CloseDoorsFromProximity();
    }
}