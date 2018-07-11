using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Weapon mainWeapon;
    public Weapon secondaryWeapon;

    public void UseMainWeapon(bool pressed)
    {
        if (mainWeapon != null)
        {
            mainWeapon.FireWeapon(pressed);
        }
    }

    public void UseSecondaryWeapon(bool pressed)
    {
        if (secondaryWeapon != null)
        {
            secondaryWeapon.FireWeapon(pressed);
        }
    }

    /*
     *  This class will also handle weapon switching etc.
     */
}
