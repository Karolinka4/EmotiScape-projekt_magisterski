using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRSimpleInteractable))]
public class GrabToSpawnClone : MonoBehaviour
{
    public GameObject clonePrefab;
    public Transform spawnPoint;
    public bool onlyOneCloneAtATime = false;

    XRSimpleInteractable _interactable;
    GameObject _currentClone;

    void Awake() => _interactable = GetComponent<XRSimpleInteractable>();

    void OnEnable() => _interactable.selectEntered.AddListener(OnSelectEntered);
    void OnDisable() => _interactable.selectEntered.RemoveListener(OnSelectEntered);

    void OnSelectEntered(SelectEnterEventArgs args)
    {
        if (clonePrefab == null) return;
        if (onlyOneCloneAtATime && _currentClone != null) return;
        StartCoroutine(SpawnAndTransferNextFrame(args));
    }

    IEnumerator SpawnAndTransferNextFrame(SelectEnterEventArgs args)
    {
        yield return null;

        var mgr = _interactable.interactionManager;
        if (mgr == null) yield break;

        if (_interactable.isSelected)
            mgr.SelectExit(args.interactorObject, _interactable);

        var t = spawnPoint != null ? spawnPoint : transform;
        _currentClone = Instantiate(clonePrefab, t.position, t.rotation);

        var cloneGrab = _currentClone.GetComponent<XRGrabInteractable>();
        if (cloneGrab == null)
        {
            Debug.LogError("clonePrefab musi mieæ XRGrabInteractable!");
            yield break;
        }

        cloneGrab.interactionManager = mgr;
        mgr.SelectEnter(args.interactorObject, cloneGrab);
    }
}
