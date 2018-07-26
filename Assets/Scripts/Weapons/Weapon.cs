using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected const float knockbackModifier = 0.5f;

    protected WeaponController weaponController;

    public abstract void FireWeapon(bool pressed);

    public bool IsActive()
    {
        if (weaponController == null)
            weaponController = GetComponentInParent<WeaponController>();

        return weaponController.IsActiveWeapon(this);
    }
}
