using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputController : MonoBehaviour
{
    private HoverMotor motor;

    [Header("Control Settings")]
    [SerializeField]
    private string xMovAxis = "Horizontal";
    [SerializeField]
    private string yMovAxis = "Vertical";
    [SerializeField]
    private string xCamAxis = "LookX";
    [SerializeField]
    private string yCamAxis = "LookY";

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        motor = GetComponent<HoverMotor>();
    }

    // Update is called once per frame
    void Update()
    {
        // Get vertical and horizontal input vectors
        if (motor != null)
        {
            motor.Move(Input.GetAxisRaw(xMovAxis), Input.GetAxisRaw(yMovAxis));
            motor.MoveCamera(Input.GetAxisRaw(xCamAxis), Input.GetAxisRaw(yCamAxis));

            if (Input.GetKey(KeyCode.R))
                motor.ChangeHeight(1f);

            if (Input.GetKey(KeyCode.F))
                motor.ChangeHeight(-1f);
        }
    }
}
