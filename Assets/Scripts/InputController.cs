using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    [SerializeField] private Motor movementMotor;
    [SerializeField] private Motor cameraMotor;

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
        movementMotor.Move(Input.GetAxisRaw(xMovAxis), Input.GetAxisRaw(yMovAxis));

        cameraMotor.Move(Input.GetAxisRaw(xCamAxis), Input.GetAxisRaw(yCamAxis));

        //// Abilities
        //// Up
        //if (Input.GetKeyDown(upAbility))
        //    movementMotor.UseUpAbility(true);

        //if (Input.GetKeyUp(upAbility))
        //    movementMotor.UseUpAbility(false);

        //// Down
        //if (Input.GetKeyDown(downAbility))
        //    movementMotor.UseDownAbility(true);

        //if (Input.GetKeyUp(downAbility))
        //    movementMotor.UseDownAbility(false);

        //// Left
        //if (Input.GetKeyDown(leftAbility))
        //    movementMotor.UseLeftAbility(true);

        //if (Input.GetKeyUp(leftAbility))
        //    movementMotor.UseLeftAbility(false);

        //// Right
        //if (Input.GetKeyDown(rightAbility))
        //    movementMotor.UseRightAbility(true);

        //if (Input.GetKeyUp(rightAbility))
        //    movementMotor.UseRightAbility(false);
    }
}
