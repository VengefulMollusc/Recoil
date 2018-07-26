using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Weapon mainWeapon;
    public Weapon secondaryWeapon;

    private bool mainWeaponState;
    private bool secondaryWeaponState;

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

    public bool IsActiveWeapon(Weapon weapon)
    {
        return weapon == mainWeapon || weapon == secondaryWeapon;
    }

    /*
     *  This class will also handle weapon switching etc.
     */
}
