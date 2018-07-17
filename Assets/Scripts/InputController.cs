using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private HoverMotor motor;
    private WeaponController weaponController;

    [Header("Axes")]
    [SerializeField]
    private string xMovAxis = "Horizontal";
    [SerializeField]
    private string yMovAxis = "Vertical";
    [SerializeField]
    private string xCamAxis = "LookX";
    [SerializeField]
    private string yCamAxis = "LookY";

    [SerializeField]
    private string mainWeaponAxis = "WeaponMain";
    [SerializeField]
    private string secondaryWeaponAxis = "WeaponSecondary";

    [Header("Buttons")]
    [SerializeField]
    private string boostButton = "Jump";
    
    [SerializeField]
    private string mainWeaponButton = "Fire1";
    [SerializeField]
    private string secondaryWeaponButton = "Fire2";


    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        motor = GetComponent<HoverMotor>();
        weaponController = GetComponent<WeaponController>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get vertical and horizontal input vectors
        if (motor != null)
        {
            // Apply movement and rotation
            motor.Move(Input.GetAxisRaw(xMovAxis), Input.GetAxisRaw(yMovAxis));
            motor.MoveCamera(Input.GetAxisRaw(xCamAxis), Input.GetAxisRaw(yCamAxis));

            // Boost
            if (Input.GetButtonDown(boostButton))
                motor.Boost(true);
            if (Input.GetButtonUp(boostButton))
                motor.Boost(false);
        }

        // get weapon input state
        if (weaponController != null)
        {
            if (Input.GetButtonDown(mainWeaponButton) || Input.GetAxisRaw(mainWeaponAxis) > 0f)
                weaponController.UseMainWeapon(true);
            if (Input.GetButtonUp(mainWeaponButton) || Input.GetAxisRaw(mainWeaponAxis) <= 0f)
                weaponController.UseMainWeapon(false);

            if (Input.GetButtonDown(secondaryWeaponButton) || Input.GetAxisRaw(secondaryWeaponAxis) > 0f)
                weaponController.UseSecondaryWeapon(true);
            if (Input.GetButtonUp(secondaryWeaponButton) || Input.GetAxisRaw(secondaryWeaponAxis) <= 0f)
                weaponController.UseSecondaryWeapon(false);
        }
    }
}
