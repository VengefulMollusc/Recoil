using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiringPoint : MonoBehaviour
{
    public bool enableParticles;
    private ParticleSystem particleSystem;

    public void Fire()
    {
        if (!enableParticles)
            return;

        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        particleSystem.Play();
    }
}
