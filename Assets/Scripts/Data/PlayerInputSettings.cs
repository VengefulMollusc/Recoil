using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class PlayerInputSettings : ScriptableObject
{
    [Header("Axes")]
    public string xMovAxis = "Horizontal";
    public string yMovAxis = "Vertical";
    public string xCamAxis = "LookX";
    public string yCamAxis = "LookY";

    public bool useWeaponAxes;
    public string mainWeaponAxis = "WeaponMain";
    public string secondaryWeaponAxis = "WeaponSecondary";

    [Header("Buttons")]
    public string boostButton = "Jump";

    public string mainWeaponButton = "Fire1";
    public string secondaryWeaponButton = "Fire2";
    public string utilityWeaponButton = "Fire3";
}
