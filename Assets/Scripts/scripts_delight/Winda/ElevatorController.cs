using System.Collections;
using UnityEngine;
using Unity.XR.CoreUtils;

public class ElevatorController : MonoBehaviour
{
    // =========================================================
    // WINDA
    // =========================================================

    [Header("Elevator")]
    [SerializeField] private Rigidbody elevatorRigidbody;


    // =========================================================
    // DRZWI
    // =========================================================

    [Header("Doors")]
    [SerializeField] private Animator doorAnimator;

    [Tooltip("Ile sekund trwa zamykanie drzwi.")]
    [SerializeField] private float doorCloseTime = 3f;


    // =========================================================
    // DèWI KI DRZWI
    // =========================================================

    [Header("Door Audio")]
    [SerializeField] private AudioSource doorAudioSource;

    [Tooltip("DüwiÍk otwierania drzwi.")]
    [SerializeField] private AudioClip doorOpenSound;

    [Tooltip("DüwiÍk zamykania drzwi.")]
    [SerializeField] private AudioClip doorCloseSound;


    // =========================================================
    // GRACZ
    // =========================================================

    [Header("Player")]
    [SerializeField] private ElevatorCabinTrigger cabinTrigger;


    // =========================================================
    // RUCH WINDY
    // =========================================================

    [Header("Movement")]

    [Tooltip("PrÍdkoúÊ p≥ynnej jazdy windy w metrach na sekundÍ.")]
    [SerializeField] private float moveSpeed = 1.5f;

    [Tooltip("O ile metrÛw winda ma zjechaÊ w dÛ≥ od pozycji poczπtkowej.")]
    [SerializeField] private float travelDistance = 6f;


    private Vector3 topPosition;
    private Vector3 bottomPosition;

    private bool isMoving = false;

    // Po dojechaniu drzwi pozostajπ otwarte,
    // aø gracz kliknie przycisk windy.
    private bool keepDoorsOpen = false;

    private XROrigin passenger;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (elevatorRigidbody == null)
        {
            Debug.LogError(
                "ElevatorController: Brakuje Rigidbody windy!"
            );

            return;
        }

        // Pozycja poczπtkowa windy = G”RNE piÍtro.
        topPosition = elevatorRigidbody.position;

        // Dolne piÍtro.
        bottomPosition =
            topPosition + Vector3.down * travelDistance;

        Debug.Log(
            "Winda: gÛra Y = " + topPosition.y +
            " | dÛ≥ Y = " + bottomPosition.y
        );
    }


    // =========================================================
    // PRZYCISK GO - P£YNNA JAZDA
    // =========================================================

    public void GoSmooth()
    {
        if (!CanTravel())
            return;

        StartCoroutine(Travel(false));
    }


    // =========================================================
    // PRZYCISK TELEPORT
    // =========================================================

    public void GoTeleport()
    {
        if (!CanTravel())
            return;

        StartCoroutine(Travel(true));
    }


    // =========================================================
    // CZY WINDA MOØE JECHA∆
    // =========================================================

    private bool CanTravel()
    {
        if (isMoving)
        {
            Debug.Log("Winda juø jedzie.");
            return false;
        }

        if (elevatorRigidbody == null)
        {
            Debug.LogError("Brak Rigidbody windy!");
            return false;
        }

        if (doorAnimator == null)
        {
            Debug.LogError("Brak Animatora drzwi!");
            return false;
        }

        if (cabinTrigger == null)
        {
            Debug.LogError("Brak CabinTrigger!");
            return false;
        }

        if (!cabinTrigger.PlayerInside)
        {
            Debug.Log(
                "Nie moøna uruchomiÊ windy - gracza nie ma w kabinie."
            );

            return false;
        }

        if (cabinTrigger.Player == null)
        {
            Debug.Log(
                "Nie moøna uruchomiÊ windy - brak referencji do gracza."
            );

            return false;
        }

        return true;
    }


    // =========================================================
    // PODR”Ø
    // =========================================================

    private IEnumerator Travel(bool teleport)
    {
        isMoving = true;

        // ZapamiÍtujemy gracza.
        passenger = cabinTrigger.Player;

        // Po klikniÍciu przycisku pozwalamy zamknπÊ drzwi.
        keepDoorsOpen = false;


        // =====================================================
        // ZAMYKANIE DRZWI
        // =====================================================

        SetDoorsOpen(false);

        Debug.Log("Winda: zamykanie drzwi");


        // Czekamy aø drzwi siÍ zamknπ.
        yield return new WaitForSeconds(doorCloseTime);


        // =====================================================
        // WYB”R PI TRA
        // =====================================================

        Vector3 destination = GetOppositeFloor();


        // =====================================================
        // JAZDA
        // =====================================================

        if (teleport)
        {
            TeleportElevator(destination);
        }
        else
        {
            yield return MoveElevator(destination);
        }


        // Ma≥a pauza po dojechaniu.
        yield return new WaitForSeconds(0.15f);


        // =====================================================
        // OTWIERANIE DRZWI
        // =====================================================

        SetDoorsOpen(true);

        Debug.Log("Winda: otwieranie drzwi po dojechaniu");


        // Drzwi pozostajπ otwarte.
        keepDoorsOpen = true;


        // Koniec podrÛøy.
        passenger = null;
        isMoving = false;
    }


    // =========================================================
    // WYBIERANIE PRZECIWNEGO PI TRA
    // =========================================================

    private Vector3 GetOppositeFloor()
    {
        Vector3 currentPosition =
            elevatorRigidbody.position;

        float distanceToTop =
            Vector3.Distance(
                currentPosition,
                topPosition
            );

        float distanceToBottom =
            Vector3.Distance(
                currentPosition,
                bottomPosition
            );


        // Jeúli bliøej gÛry -> jedziemy na dÛ≥.
        if (distanceToTop < distanceToBottom)
        {
            Debug.Log(
                "Winda jest na G”RZE -> jedzie na D”£"
            );

            return bottomPosition;
        }


        // Jeúli bliøej do≥u -> jedziemy na gÛrÍ.
        Debug.Log(
            "Winda jest na DOLE -> jedzie na G”R "
        );

        return topPosition;
    }


    // =========================================================
    // P£YNNA JAZDA
    // =========================================================

    private IEnumerator MoveElevator(Vector3 destination)
    {
        while (
            Vector3.Distance(
                elevatorRigidbody.position,
                destination
            ) > 0.005f
        )
        {
            yield return new WaitForFixedUpdate();


            Vector3 oldPosition =
                elevatorRigidbody.position;


            Vector3 newPosition =
                Vector3.MoveTowards(
                    oldPosition,
                    destination,
                    moveSpeed * Time.fixedDeltaTime
                );


            Vector3 movement =
                newPosition - oldPosition;


            // Przesuwamy windÍ.
            elevatorRigidbody.MovePosition(newPosition);


            // Przesuwamy gracza razem z windπ.
            MovePassenger(movement);
        }


        // Dok≥adne ustawienie windy na koÒcu.
        Vector3 finalMovement =
            destination - elevatorRigidbody.position;


        elevatorRigidbody.position = destination;


        // Dok≥adne przesuniÍcie gracza.
        MovePassenger(finalMovement);
    }


    // =========================================================
    // TELEPORT WINDY
    // =========================================================

    private void TeleportElevator(Vector3 destination)
    {
        Vector3 oldPosition =
            elevatorRigidbody.position;


        Vector3 movement =
            destination - oldPosition;


        // Teleport windy.
        elevatorRigidbody.position = destination;


        // Teleport gracza.
        TeleportPassenger(movement);
    }


    // =========================================================
    // GRACZ PODCZAS NORMALNEJ JAZDY
    // =========================================================

    private void MovePassenger(Vector3 movement)
    {
        if (passenger == null)
            return;


        CharacterController controller =
            passenger.GetComponent<CharacterController>();


        if (
            controller != null &&
            controller.enabled
        )
        {
            controller.Move(movement);
        }
        else
        {
            passenger.transform.position += movement;
        }
    }


    // =========================================================
    // GRACZ PODCZAS TELEPORTU
    // =========================================================

    private void TeleportPassenger(Vector3 movement)
    {
        if (passenger == null)
            return;


        CharacterController controller =
            passenger.GetComponent<CharacterController>();


        if (controller != null)
        {
            bool wasEnabled =
                controller.enabled;


            // Wy≥πczamy CharacterController,
            // øeby nie blokowa≥ teleportu.
            controller.enabled = false;


            passenger.transform.position += movement;


            controller.enabled = wasEnabled;
        }
        else
        {
            passenger.transform.position += movement;
        }
    }


    // =========================================================
    // OTWIERANIE DRZWI PRZEZ TRIGGER
    // =========================================================

    public void OpenDoorsFromProximity()
    {
        // Podczas jazdy trigger nic nie robi.
        if (isMoving)
            return;


        SetDoorsOpen(true);
    }


    // =========================================================
    // ZAMYKANIE DRZWI PRZEZ TRIGGER
    // =========================================================

    public void CloseDoorsFromProximity()
    {
        // Podczas jazdy trigger nic nie robi.
        if (isMoving)
            return;


        // Po dojechaniu drzwi majπ zostaÊ otwarte.
        if (keepDoorsOpen)
            return;


        // Jeøeli gracz jest wewnπtrz windy,
        // drzwi pozostajπ otwarte.
        //
        // Zamknπ siÍ dopiero po klikniÍciu
        // przycisku GO / TELEPORT.
        if (
            cabinTrigger != null &&
            cabinTrigger.PlayerInside
        )
        {
            return;
        }


        SetDoorsOpen(false);
    }


    // =========================================================
    // OTWIERANIE / ZAMYKANIE DRZWI + DèWI K
    // =========================================================

    private void SetDoorsOpen(bool open)
    {
        if (doorAnimator == null)
        {
            Debug.LogError(
                "ElevatorController: Brak Animatora drzwi!"
            );

            return;
        }


        // Sprawdzamy aktualny stan.
        bool currentlyOpen =
            doorAnimator.GetBool("Open");


        // Jeøeli drzwi juø sπ w tym stanie,
        // nic ponownie nie odpalamy.
        //
        // Zapobiega to wielokrotnemu odtwarzaniu
        // düwiÍku przez trigger.
        if (currentlyOpen == open)
            return;


        // Zmieniamy stan animacji.
        doorAnimator.SetBool("Open", open);


        // Odtwarzamy odpowiedni düwiÍk.
        PlayDoorSound(open);
    }


    // =========================================================
    // DèWI K DRZWI
    // =========================================================

    private void PlayDoorSound(bool opening)
    {
        if (doorAudioSource == null)
        {
            Debug.LogWarning(
                "ElevatorController: Brak Door Audio Source!"
            );

            return;
        }


        AudioClip clip;


        if (opening)
        {
            clip = doorOpenSound;
        }
        else
        {
            clip = doorCloseSound;
        }


        if (clip == null)
        {
            if (opening)
            {
                Debug.LogWarning(
                    "ElevatorController: Brak düwiÍku OTWIERANIA drzwi!"
                );
            }
            else
            {
                Debug.LogWarning(
                    "ElevatorController: Brak düwiÍku ZAMYKANIA drzwi!"
                );
            }

            return;
        }


        // Jeúli jeszcze gra poprzedni düwiÍk,
        // zatrzymujemy go.
        doorAudioSource.Stop();


        doorAudioSource.clip = clip;
        doorAudioSource.Play();
    }
}