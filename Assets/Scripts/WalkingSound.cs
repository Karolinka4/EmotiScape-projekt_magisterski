using UnityEngine;

public class WalkingSound : MonoBehaviour
{
    public CharacterController characterController;
    public AudioSource walkingAudio;

    public float minimumSpeed = 0.1f;

    void Update()
    {
        Vector3 velocity = characterController.velocity;
        velocity.y = 0;

        float speed = velocity.magnitude;

        if (speed > minimumSpeed && characterController.isGrounded)
        {
            if (!walkingAudio.isPlaying)
            {
                walkingAudio.Play();
            }
        }
        else
        {
            if (walkingAudio.isPlaying)
            {
                walkingAudio.Stop();
            }
        }
    }
}