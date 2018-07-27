using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UtilityWeapon : Weapon
{
    protected Weapon utilityReferenceWeapon;

    public void SetUtilityReferenceWeapon(Weapon weapon)
    {
        utilityReferenceWeapon = weapon;
    }
}
