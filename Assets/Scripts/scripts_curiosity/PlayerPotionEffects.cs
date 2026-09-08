using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class PlayerPotionEffects : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Tutaj przypisz g³ówny XR Origin gracza.")]
    public Transform playerRoot;

    [Header("Shrink")]
    [Range(0.05f, 1f)]
    public float shrinkMultiplier = 0.3f;

    public float shrinkDuration = 20f;

    Coroutine shrinkRoutine;

    Vector3 scaleBeforeShrink;

    bool shrinkActive;

    [Header("Black & White")]
    public Volume blackWhiteVolume;
    public float blackWhiteDuration = 20f;

    Coroutine blackWhiteRoutine;

    [Header("Levitation")]
    public LevitationManager levitationManager;

    [Header("Upside Down World")]
    public UpsideDownWorldManager upsideDownWorldManager;

    void Awake()
    {
        // Je¿eli skrypt jest bezpoœrednio
        // na XR Origin, nie musisz nic przypisywaæ.
        if (!playerRoot)
            playerRoot = transform;
    }

    public void ApplyPotion(
        PotionEffectType effect
    )
    {
        switch (effect)
        {
            case PotionEffectType.Shrink:
                StartShrink();
                break;

            case PotionEffectType.BlackWhite:
                StartBlackWhite();
                break;

            case PotionEffectType.LevitateObjects:
                StartLevitateObjects();
                break;

            case PotionEffectType.UpsideDownWorld:
                StartUpsideDownWorld();
                break;

            case PotionEffectType.None:
                break;
        }
    }

    void StartShrink()
    {
        if (!playerRoot)
        {
            Debug.LogError(
                "PlayerPotionEffects: brak Player Root!"
            );

            return;
        }

        // Je¿eli gracz ju¿ jest ma³y
        // i wypije kolejn¹ miksturê,
        // NIE zmniejszamy go ponownie.
        //
        // Po prostu restartujemy licznik 20 sekund.
        if (shrinkActive)
        {
            if (shrinkRoutine != null)
                StopCoroutine(shrinkRoutine);

            shrinkRoutine =
                StartCoroutine(ShrinkTimer());

            return;
        }

        // Zapamiêtujemy DOK£ADNIE skalê
        // sprzed wypicia mikstury.
        scaleBeforeShrink =
            playerRoot.localScale;

        // Zmniejszenie
        playerRoot.localScale =
            scaleBeforeShrink * shrinkMultiplier;

        shrinkActive = true;

        shrinkRoutine =
            StartCoroutine(ShrinkTimer());
    }

    IEnumerator ShrinkTimer()
    {
        yield return new WaitForSeconds(
            shrinkDuration
        );

        // Powrót dok³adnie do skali
        // sprzed wypicia.
        if (playerRoot)
        {
            playerRoot.localScale =
                scaleBeforeShrink;
        }

        shrinkActive = false;
        shrinkRoutine = null;
    }

    void OnDisable()
    {
        // Zabezpieczenie, ¿eby gracz
        // nie zosta³ przypadkiem ma³y,
        // gdy komponent zostanie wy³¹czony.
        if (shrinkActive && playerRoot)
        {
            playerRoot.localScale =
                scaleBeforeShrink;
        }

        shrinkActive = false;
        shrinkRoutine = null;
    }
    void StartBlackWhite()
    {
        if (!blackWhiteVolume)
        {
            Debug.LogError(
                "PlayerPotionEffects: nie przypisano Black White Volume!"
            );

            return;
        }

        // W³¹cz efekt
        blackWhiteVolume.weight = 1f;

        // Jeœli ju¿ dzia³a³, restartujemy licznik
        if (blackWhiteRoutine != null)
        {
            StopCoroutine(blackWhiteRoutine);
        }

        blackWhiteRoutine =
            StartCoroutine(BlackWhiteTimer());
    }

    IEnumerator BlackWhiteTimer()
    {
        yield return new WaitForSeconds(
            blackWhiteDuration
        );

        // Wy³¹cz efekt i wróæ do normalnych kolorów
        if (blackWhiteVolume)
        {
            blackWhiteVolume.weight = 0f;
        }

        blackWhiteRoutine = null;
    }

    void StartLevitateObjects()
    {
        if (!levitationManager)
        {
            Debug.LogError(
                "PlayerPotionEffects: nie przypisano LevitationManager!"
            );

            return;
        }

        levitationManager.StartLevitation();
    }


    void StartUpsideDownWorld()
    {
        if (!upsideDownWorldManager)
        {
            Debug.LogError(
                "PlayerPotionEffects: nie przypisano UpsideDownWorldManager!"
            );

            return;
        }

        upsideDownWorldManager
            .StartUpsideDownWorld();
    }

}