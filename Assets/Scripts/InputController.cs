using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private HoverMotor motor;
    private WeaponController weaponController;

    private PlayerInputSettings inputSettings;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        motor = GetComponent<HoverMotor>();
        weaponController = GetComponent<WeaponController>();
    }

    public void SetInputSettings(PlayerInputSettings inputSettings)
    {
        this.inputSettings = inputSettings;
    }

    // Update is called once per frame
    void Update()
    {
        if (inputSettings == null)
            return;

        // Get vertical and horizontal input vectors
        if (motor != null)
        {
            // Apply movement and rotation
            motor.Move(Input.GetAxisRaw(inputSettings.xMovAxis), Input.GetAxisRaw(inputSettings.yMovAxis));
            motor.MoveCamera(Input.GetAxisRaw(inputSettings.xCamAxis), Input.GetAxisRaw(inputSettings.yCamAxis));

            // Boost
            if (Input.GetButtonDown(inputSettings.boostButton))
                motor.Boost(true);
            if (Input.GetButtonUp(inputSettings.boostButton))
                motor.Boost(false);
        }

        // get weapon input state
        if (weaponController != null)
        {
            if (inputSettings.useWeaponAxes)
            {
                if (Input.GetAxisRaw(inputSettings.mainWeaponAxis) > 0f)
                    weaponController.UseMainWeapon(true);
                if (Input.GetAxisRaw(inputSettings.mainWeaponAxis) <= 0f)
                    weaponController.UseMainWeapon(false);

                if (Input.GetAxisRaw(inputSettings.secondaryWeaponAxis) > 0f)
                    weaponController.UseSecondaryWeapon(true);
                if (Input.GetAxisRaw(inputSettings.secondaryWeaponAxis) <= 0f)
                    weaponController.UseSecondaryWeapon(false);
            }
            else
            {
                if (Input.GetButtonDown(inputSettings.mainWeaponButton))
                    weaponController.UseMainWeapon(true);
                if (Input.GetButtonUp(inputSettings.mainWeaponButton))
                    weaponController.UseMainWeapon(false);

                if (Input.GetButtonDown(inputSettings.secondaryWeaponButton))
                    weaponController.UseSecondaryWeapon(true);
                if (Input.GetButtonUp(inputSettings.secondaryWeaponButton))
                    weaponController.UseSecondaryWeapon(false);
            }
        }
    }
}
