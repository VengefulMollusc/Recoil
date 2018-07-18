using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleHealthController : HealthController
{
    public float healthAmount;

    public override void Damage(float damageAmount)
    {
        if (healthAmount <= 0)
            return;

        healthAmount -= damageAmount;

        Debug.Log(healthAmount);

        if (healthAmount <= 0f)
        {
            healthAmount = 0f;
            Debug.Log("Deaded");
        }
    }

    public override bool isDead()
    {
        return healthAmount <= 0f;
    }
}
