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

    [Header("Buttons")]
    [SerializeField]
    private KeyCode boostKey = KeyCode.LeftShift;
    [SerializeField]
    private KeyCode increaseHeightKey = KeyCode.R;
    [SerializeField]
    private KeyCode decreaseHeightKey = KeyCode.F;

    [SerializeField]
    private KeyCode mainWeaponKey = KeyCode.Space;
    [SerializeField]
    private KeyCode secondaryWeaponKey = KeyCode.E;

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

            // Height change buttons
            if (Input.GetKey(increaseHeightKey))
                motor.ChangeHeight(1f);

            if (Input.GetKey(decreaseHeightKey))
                motor.ChangeHeight(-1f);

            // Boost
            if (Input.GetKeyDown(boostKey))
                motor.Boost(true);
            if (Input.GetKeyUp(boostKey))
                motor.Boost(false);
        }

        if (weaponController != null)
        {
            if (Input.GetKeyDown(mainWeaponKey))
                weaponController.UseMainWeapon(true);
            if (Input.GetKeyUp(mainWeaponKey))
                weaponController.UseMainWeapon(false);

            if (Input.GetKeyDown(secondaryWeaponKey))
                weaponController.UseSecondaryWeapon(true);
            if (Input.GetKeyUp(secondaryWeaponKey))
                weaponController.UseSecondaryWeapon(false);
        }
    }
}
