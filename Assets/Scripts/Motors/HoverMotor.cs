using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float dampenForceFactor = 0.1f;
    [SerializeField] private float leanStrength = 0.5f;

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float baseAngularDrag = 4f;
    [SerializeField] private float turningAngularDrag = 2f;

    [Header("Hover")]
    [SerializeField] private float hoverHeight = 15f;
    [SerializeField] private float minHoverHeight = 10f;
    [SerializeField] private float maxHoverHeight = 20f;
    [SerializeField] private float heightChangeForce = 3f;
    [SerializeField] private float heightChangeRate = 10f;
    [SerializeField] private float hoverForce = 30f;
    [SerializeField] private float gravityForce = 10f;

    [Header("Gyro")]
    [SerializeField] private float gyroCorrectionStrength = 6f;
    [SerializeField] private float rotationLimit = 0.6f;
    [SerializeField] private float rotationCorrectionStrength = 4f;

    private Rigidbody rb;
    private Vector2 moveInputVector;
    private Vector2 turnInputVector;
    private Vector3 gravityVector;

    private Vector3 forward;
    private Vector3 up;
    private Vector3 right;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityVector = Vector3.down * gravityForce;

        // sqr maxspeed for cheaper comparisons later
        maxSpeed *= maxSpeed;

        // update cached transform variables
        forward = transform.forward;
        up = transform.up;
        right = transform.right;
    }

    public void Move(float x, float y)
    {
        moveInputVector = new Vector2(x, y);
        if (moveInputVector.sqrMagnitude > 1f)
            moveInputVector.Normalize();
    }

    public void MoveCamera(float x, float y)
    {
        turnInputVector = new Vector2(x, y);
    }

    public void ChangeHeight(float change)
    {
        // Change hover height between limits
        hoverHeight = Mathf.Clamp(hoverHeight + (change * heightChangeRate * Time.deltaTime), 
            minHoverHeight, maxHoverHeight);

        // boost hover force in direction of change
        rb.AddForce(Vector3.up * change * heightChangeForce * Time.deltaTime, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        // update cached transform variables
        forward = transform.forward;
        up = transform.up;
        right = transform.right;

        // Apply Gravity
        rb.AddForce(gravityVector * Time.fixedDeltaTime, ForceMode.Impulse);

        // Movement
        ApplyMovementForce();
        DampenMovement();

        // Turning
        ApplyTurningForce();
        DampenTurning();

        // Hover force and gyro correction
        ApplyHoverForce();
        GyroCorrection();
    }

    void ApplyMovementForce()
    {
        // Apply force to move tank
        if (moveInputVector == Vector2.zero)
            return;

        // Get movement axes relative to global axes rather than local vertical look direction
        Vector3 forwardFlat = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, forwardFlat);
        Vector3 rightFlat = rot * Vector3.right;

        Vector3 inputForce = (forwardFlat * moveInputVector.y)
            + (rightFlat * moveInputVector.x);
        rb.AddForce(inputForce * moveForce * Time.fixedDeltaTime, ForceMode.Impulse);

        // lean slightly to match left/right movement
        Vector3 leanTorque = forwardFlat * -moveInputVector.x * leanStrength;
        rb.AddTorque(leanTorque * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void DampenMovement()
    {
        // Dampens horizontal movement slightly to limit speed
        if (moveInputVector != Vector2.zero)
            return;

        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
        float dampenForce = Utilities.MapValues(flatVel.sqrMagnitude, 0f, maxSpeed, 0.1f, moveForce * dampenForceFactor);
        rb.AddForce(-flatVel.normalized * dampenForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void ApplyTurningForce()
    {
        // Apply torque to rotate facing direction
        if (turnInputVector == Vector2.zero)
            return;

        Vector3 torque = (up * turnInputVector.x * turnSpeed) + (right * -turnInputVector.y * turnSpeed);

        rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void DampenTurning()
    {
        // Apply angular drag value based on input state
        rb.angularDrag = (turnInputVector == Vector2.zero) ? baseAngularDrag : turningAngularDrag;
    }

    void ApplyHoverForce()
    {
        // raycast down and apply hover force relative to height
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, hoverHeight))
        {
            float distanceToGround = hitInfo.distance;
            float force = Utilities.MapValues(distanceToGround, hoverHeight, 0f, 0f, hoverForce);
            rb.AddForce(Vector3.up * force * Time.fixedDeltaTime, ForceMode.Impulse);
        }
    }

    void GyroCorrection()
    {
        // check against vertical rotation limit
        float verticalDot = Vector3.Dot(forward, Vector3.up);
        if (Mathf.Abs(verticalDot) > rotationLimit)
        {
            // correct vertical rotation
            // map correction strength to 0-1
            float correctionStrength = Utilities.MapValues(Mathf.Abs(verticalDot), rotationLimit, 1f, 0f, 1f);
            if (verticalDot < 0f)
                correctionStrength *= -1;

            Vector3 correctionTorque = Vector3.Cross(Vector3.up, forward).normalized * rotationCorrectionStrength *
                                       correctionStrength;

            rb.AddTorque(correctionTorque * Time.fixedDeltaTime, ForceMode.Impulse);
        }
        else
        {
            // Correct gyro
            // rotate around transform.forward until transform.up is closest to V3.up
            Vector3 projectionPlaneNormal = Vector3.Cross(Vector3.up, forward);

            float gyroDot = Vector3.Dot(up, projectionPlaneNormal);

            if (up.y < 0f)
                gyroDot = gyroDot < 0f ? -1f : 1f;

            Vector3 gyroTorque = forward * Time.fixedDeltaTime * (gyroDot * gyroCorrectionStrength);
            rb.AddTorque(gyroTorque, ForceMode.Impulse);
        }
    }
}