using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHealthController : HealthController
{
    public float healthMax;

    private float currentHealth;

    private bool canDie;

    private void Start()
    {
        ResetHealth();
    }

    public override void Damage(float damageAmount)
    {
        if (currentHealth <= 0)
            return;

        currentHealth -= damageAmount;

        if (currentHealth <= 0)
        {
            currentHealth = 0f;
            Debug.Log(gameObject.name + " Deaded");
        }
    }

    public override void Damage(float damageAmount, float deathDelay)
    {
        Damage(damageAmount);
        if (IsDead())
            StartCoroutine(DelayDeathEffect(deathDelay));

    }

    private IEnumerator DelayDeathEffect(float delay)
    {
        canDie = false;
        yield return new WaitForSeconds(delay);
        canDie = true;
    }

    public override bool IsDead()
    {
        return currentHealth <= 0f && canDie;
    }

    public override void ResetHealth()
    {
        currentHealth = healthMax;
        canDie = true;
    }
}
