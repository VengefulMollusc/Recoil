using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : Motor
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float hoverHeight = 5f;
    [SerializeField] private float hoverForce = 20f;
    [SerializeField] private float gravityForce = 10f;
    [SerializeField] private float dampenForceFactor = 0.4f;

    [Header("Camera")]
    [SerializeField] private float turnSpeed = 1f;

    private Rigidbody rb;
    private Vector2 moveInputVector;
    private Vector2 turnInputVector;
    private Vector3 gravityVector;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityVector = Vector3.down * gravityForce;

        // sqr maxspeed for comparisons later
        maxSpeed *= maxSpeed;
    }

    void FixedUpdate()
    {
        // Apply Gravity
        rb.AddForce(gravityVector * Time.fixedDeltaTime, ForceMode.Impulse);

        ApplyMovementForce();
        DampenMovement();

        ApplyTurningForce();
        DampenTurning();

        ApplyHoverForce();
    }

    void ApplyMovementForce()
    {
        if (moveInputVector == Vector2.zero)
            return;

        Vector3 inputForce = (transform.forward * moveInputVector.y) 
            + (transform.right * moveInputVector.x);
        rb.AddForce(inputForce * moveForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void DampenMovement()
    {
        //if (moveInputVector != Vector2.zero)
        //    return;

        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
        float dampenForce = Utilities.MapValues(flatVel.sqrMagnitude, 0f, maxSpeed, 0.1f, moveForce * dampenForceFactor);
        rb.AddForce(-flatVel.normalized * dampenForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void ApplyTurningForce()
    {
        if (turnInputVector == Vector2.zero)
            return;

        Vector3 torque = (transform.up * turnInputVector.x * turnSpeed) + (transform.right * turnInputVector.y * turnSpeed);

        rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void DampenTurning()
    {
        
    }

    void ApplyHoverForce()
    {
        // raycast down and apply hover force
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, hoverHeight))
        {
            float distanceToGround = hitInfo.distance;
            float force = Utilities.MapValues(distanceToGround, hoverHeight, 0f, 0f, hoverForce);
            rb.AddForce(Vector3.up * force * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }

    public override void Move(float x, float y)
    {
        moveInputVector = new Vector2(x, y);
        if (moveInputVector.sqrMagnitude > 1f)
            moveInputVector.Normalize();
    }

    public override void MoveCamera(float x, float y)
    {
        turnInputVector = new Vector2(x, y);
    }
}