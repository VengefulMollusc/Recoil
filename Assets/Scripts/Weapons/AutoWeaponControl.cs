using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoWeaponControl : MonoBehaviour
{
    public bool continuousFire;

    [Header("Burst/timed fire options")]
    public float fireDuration;
    public float pauseDuration;

    private const float updateInterval = 0.1f;

    private Weapon weapon;
    private bool fireState;
    private bool firing;

    private float timer;
    
    void OnEnable()
    {
        weapon = GetComponent<Weapon>();

        if (!continuousFire)
            InvokeRepeating("UpdateFiring", Random.Range(0, updateInterval), updateInterval);
    }

    void OnDisable()
    {
        CancelInvoke("UpdateFiring");
    }

    public void SetOwner(GameObject owner)
    {
        weapon.SetOwner(owner);
    }

    public void SetFireState(bool state)
    {
        if (continuousFire)
        {
            weapon.FireWeapon(state);
            return;
        }

        fireState = state;
        if (fireState)
        {
            firing = true;
            weapon.FireWeapon(firing);
            timer = fireDuration;
        }
    }

    void UpdateFiring()
    {
        if (!fireState)
        {
            if (firing)
            {
                firing = false;
                weapon.FireWeapon(firing);
            }
            return;
        }

        timer -= updateInterval;
        if (timer <= 0f)
        {
            if (firing)
            {
                firing = false;
                timer = pauseDuration;
            }
            else
            {
                firing = true;
                timer = fireDuration;
            }

            weapon.FireWeapon(firing);
        }
    }

    void OnValidate()
    {
        if (fireDuration < updateInterval)
            fireDuration = updateInterval;

        if (pauseDuration < updateInterval)
            pauseDuration = updateInterval;
    }
}
