using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHealthController : HealthController
{
    public float healthMax;

    private float currentHealth;

    private void Start()
    {
        currentHealth = healthMax;
    }

    public override void Damage(float damageAmount)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damageAmount;

        Debug.Log(currentHealth);

        if (currentHealth <= 0f)
        {
            currentHealth = 0f;
            Debug.Log("Deaded");
        }
    }

    public override bool isDead()
    {
        return currentHealth <= 0f;
    }

    public override void ResetHealth()
    {
        currentHealth = healthMax;
    }
}
