using UnityEngine;

public class SnapToSlot : MonoBehaviour
{
    private bool isOccupied = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Seed") && !isOccupied)
        {
            SnapSeed(other.gameObject);
        }
    }

    void SnapSeed(GameObject seed)
    {
        isOccupied = true;

        // 1. Zapamiêtujemy oryginaln¹ skalê ziarna, zanim zmienimy rodzica
        Vector3 originalScale = seed.transform.localScale;

        // 2. Wy³¹czamy fizykê ca³kowicie
        Rigidbody rb = seed.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
            rb.useGravity = false; // Dodatkowe zabezpieczenie przed spadaniem
        }

        // 3. Przyczepiamy ziarno do slotu
        seed.transform.SetParent(this.transform);

        // 4. Resetujemy pozycjê i PRZYWRACAMY oryginaln¹ skalê
        seed.transform.localPosition = Vector3.zero;
        seed.transform.localRotation = Quaternion.identity;
        seed.transform.localScale = originalScale; // To naprawi problem "zmniejszania"

        Debug.Log("Ziarno posadzone i skala naprawiona!");
    }
}