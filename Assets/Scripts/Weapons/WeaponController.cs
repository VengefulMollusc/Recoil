using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public DamageWeapon mainWeapon;
    public DamageWeapon secondaryWeapon;
    public UtilityWeapon utilityWeapon;

    private GameObject owner;

    private bool mainWeaponState;
    private bool secondaryWeaponState;
    private bool utilityWeaponState;

    void Start()
    {
        owner = gameObject;
        foreach (LockOnTarget target in GetComponentsInChildren<LockOnTarget>())
        {
            target.SetOwner(owner);
        }
        foreach (Weapon weapon in GetComponentsInChildren<Weapon>())
        {
            weapon.SetOwner(owner);
        }

        utilityWeapon.SetUtilityReferenceWeapon(mainWeapon);
    }

    public void UseMainWeapon(bool pressed)
    {
        if (mainWeapon != null && pressed != mainWeaponState)
        {
            mainWeapon.FireWeapon(pressed);
            mainWeaponState = pressed;
        }
    }

    public void UseSecondaryWeapon(bool pressed)
    {
        if (secondaryWeapon != null && pressed != secondaryWeaponState)
        {
            secondaryWeapon.FireWeapon(pressed);
            secondaryWeaponState = pressed;
        }
    }

    public void UseUtilityWeapon(bool pressed)
    {
        if (utilityWeapon != null && pressed != utilityWeaponState)
        {
            utilityWeapon.FireWeapon(pressed);
            utilityWeaponState = pressed;
        }
    }

    public bool IsActiveWeapon(Weapon weapon)
    {
        return weapon == mainWeapon || weapon == secondaryWeapon;
    }

    /*
     *  This class will also handle weapon switching etc.
     */
}
