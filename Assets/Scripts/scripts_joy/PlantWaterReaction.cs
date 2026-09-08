using UnityEngine;

public class PlantWaterReaction : MonoBehaviour
{
    public Animator animator;
    private bool isOpen = false;

    // Ta funkcja reaguje na cz¹steczki Particle System
    private void OnParticleCollision(GameObject other)
    {
        // Sprawdzamy, czy to co w nas uderzy³o ma tag Water
        if (other.CompareTag("Water") && !isOpen)
        {
            Debug.Log("Woda (cz¹steczki) dotknê³a roœliny!");
            OpenPlant();
        }
    }

    void OpenPlant()
    {
        if (isOpen) return;

        animator.SetTrigger("OpenTrigger");
        isOpen = true;
    }
}
