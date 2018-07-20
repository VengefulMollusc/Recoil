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

        if (IsDead())
        {
            currentHealth = 0f;
            Debug.Log(gameObject.name + " Deaded");
        }
    }

    public override bool IsDead()
    {
        return currentHealth <= 0f;
    }

    public override void ResetHealth()
    {
        currentHealth = healthMax;
    }
}
