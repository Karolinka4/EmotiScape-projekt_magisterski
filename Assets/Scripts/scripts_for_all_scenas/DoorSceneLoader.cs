using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DoorSceneLoader : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("Ustawienia Drzwi")]
    [Tooltip("G³ówny obiekt drzwi (z pivotem na zawiasie)")]
    public Transform doorTransform;

    [Tooltip("Wybierz oœ obrotu zawiasu")]
    public RotationAxis rotationAxis = RotationAxis.Z;

    [Tooltip("Zaznacz, jeœli drzwi otwieraj¹ siê w odwrotn¹ stronê")]
    public bool invertDirection = false;

    [Tooltip("K¹t otwarcia wymagany do zmiany sceny (np. 15 stopni)")]
    public float targetOpenAngle = 15f;

    [Header("Zmiana Sceny")]
    public string targetSceneName;
    public float delayBeforeLoad = 0.5f;

    private XRBaseInteractable interactable;
    private Transform interactorTransform;
    private bool isGrabbed = false;
    private bool isTransitioning = false;
    private Quaternion initialDoorRotation;
    private Vector3 initialGrabPos;

    void Awake()
    {
        interactable = GetComponent<XRBaseInteractable>();

        if (doorTransform == null)
        {
            doorTransform = transform.parent;
        }
        initialDoorRotation = doorTransform.localRotation;

        // Blokada fizyki, aby klamka nie spada³a
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
    }

    void OnEnable()
    {
        interactable.selectEntered.AddListener(OnGrab);
        interactable.selectExited.AddListener(OnRelease);
    }

    void OnDisable()
    {
        interactable.selectEntered.RemoveListener(OnGrab);
        interactable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (isTransitioning) return;

        interactorTransform = args.interactorObject.transform;
        initialGrabPos = interactorTransform.position;
        isGrabbed = true;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        interactorTransform = null;
    }

    void Update()
    {
        if (!isGrabbed || isTransitioning || interactorTransform == null) return;

        // Odleg³oœæ poci¹gniêcia rêk¹ od momentu chwytu
        float pullDistance = Vector3.Distance(initialGrabPos, interactorTransform.position);

        // Poci¹gniêcie o ok. 25 cm otwiera drzwi do pe³nego k¹ta targetOpenAngle
        float angle = Mathf.Lerp(0f, targetOpenAngle, pullDistance / 0.25f);
        float clampedAngle = Mathf.Clamp(angle, 0f, targetOpenAngle);

        if (invertDirection)
        {
            clampedAngle = -clampedAngle;
        }

        // Zastosowanie rotacji w wybranej osi (domyœlnie Z)
        Vector3 euler = Vector3.zero;
        switch (rotationAxis)
        {
            case RotationAxis.X: euler = new Vector3(clampedAngle, 0f, 0f); break;
            case RotationAxis.Y: euler = new Vector3(0f, clampedAngle, 0f); break;
            case RotationAxis.Z: euler = new Vector3(0f, 0f, clampedAngle); break;
        }

        doorTransform.localRotation = initialDoorRotation * Quaternion.Euler(euler);

        // Wywo³anie zmiany sceny po osi¹gniêciu k¹ta
        if (Mathf.Abs(clampedAngle) >= targetOpenAngle)
        {
            StartCoroutine(TriggerSceneChange());
        }
    }

    private IEnumerator TriggerSceneChange()
    {
        isTransitioning = true;
        isGrabbed = false;

        // Zwolnienie chwytu kontrolera
        if (interactable.isSelected)
        {
            var interactor = interactable.firstInteractorSelecting;
            if (interactor != null)
            {
                interactable.interactionManager.SelectExit(interactor, interactable);
            }
        }
        interactable.enabled = false;

        yield return new WaitForSeconds(delayBeforeLoad);

        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}