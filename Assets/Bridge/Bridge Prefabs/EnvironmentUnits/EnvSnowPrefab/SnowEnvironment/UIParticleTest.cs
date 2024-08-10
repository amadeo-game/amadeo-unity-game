using UnityEngine;

public class UIParticleTest : MonoBehaviour
{
    private ParticleSystem particleSystem;

    void Start()
    {
        particleSystem = GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            particleSystem.Play(); // Ensure particles start playing
            Debug.Log("Particle system started.");
        }
        else
        {
            Debug.LogError("No ParticleSystem found on the GameObject.");
        }
    }
}