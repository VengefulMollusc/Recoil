using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    protected const float knockbackModifier = 0.5f;

    public abstract void FireWeapon(bool pressed);
}
