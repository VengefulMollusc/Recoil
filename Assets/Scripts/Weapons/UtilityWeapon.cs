using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class UtilityWeapon : Weapon
{
    protected DamageWeapon utilityReferenceWeapon;

    public virtual void SetUtilityReferenceWeapon(DamageWeapon weapon)
    {
        utilityReferenceWeapon = weapon;
    }
}
