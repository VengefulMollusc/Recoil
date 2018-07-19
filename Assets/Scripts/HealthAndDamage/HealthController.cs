﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HealthController : MonoBehaviour
{
    public abstract void Damage(float damageAmount);

    public abstract bool isDead();

    public abstract void ResetHealth();
}
