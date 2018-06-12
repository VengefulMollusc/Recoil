using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private Motor motor;

    [Header("Control Settings")]
    [SerializeField]
    private string xMovAxis = "Horizontal";
    [SerializeField]
    private string yMovAxis = "Vertical";
    [SerializeField]
    private string xCamAxis = "LookX";
    [SerializeField]
    private string yCamAxis = "LookY";

    private KeyCode upAbility = KeyCode.I;
    private KeyCode downAbility = KeyCode.K;
    private KeyCode leftAbility = KeyCode.J;
    private KeyCode rightAbility = KeyCode.L;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Get vertical and horizontal input vectors
        if (motor != null)
        {
            motor.Move(Input.GetAxisRaw(xMovAxis), Input.GetAxisRaw(yMovAxis));
            motor.MoveCamera(Input.GetAxisRaw(xCamAxis), Input.GetAxisRaw(yCamAxis));
        }

        //// Abilities
        //// Up
        //if (Input.GetKeyDown(upAbility))
        //    motor.UseUpAbility(true);

        //if (Input.GetKeyUp(upAbility))
        //    motor.UseUpAbility(false);

        //// Down
        //if (Input.GetKeyDown(downAbility))
        //    motor.UseDownAbility(true);

        //if (Input.GetKeyUp(downAbility))
        //    motor.UseDownAbility(false);

        //// Left
        //if (Input.GetKeyDown(leftAbility))
        //    motor.UseLeftAbility(true);

        //if (Input.GetKeyUp(leftAbility))
        //    motor.UseLeftAbility(false);

        //// Right
        //if (Input.GetKeyDown(rightAbility))
        //    motor.UseRightAbility(true);

        //if (Input.GetKeyUp(rightAbility))
        //    motor.UseRightAbility(false);
    }
}
