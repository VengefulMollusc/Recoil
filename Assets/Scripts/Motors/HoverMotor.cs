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
    [SerializeField] private float heightChangeRate = 10f;
    [SerializeField] private float hoverForce = 20f;
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

        // sqr maxspeed for comparisons later
        maxSpeed *= maxSpeed;

        // update transform variables
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
        hoverHeight = Mathf.Clamp(hoverHeight + (change * heightChangeRate * Time.deltaTime), 
            minHoverHeight, maxHoverHeight);

        // boost hover force in direction of change
        rb.AddForce(Vector3.up * change * 2f * Time.deltaTime, ForceMode.Impulse);
    }

    void FixedUpdate()
    {
        // update transform variables
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
        if (moveInputVector == Vector2.zero)
            return;

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
        if (moveInputVector != Vector2.zero)
            return;

        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
        float dampenForce = Utilities.MapValues(flatVel.sqrMagnitude, 0f, maxSpeed, 0.1f, moveForce * dampenForceFactor);
        rb.AddForce(-flatVel.normalized * dampenForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void ApplyTurningForce()
    {
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
        // raycast down and apply hover force
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
            Vector3 correctionTorque = Vector3.Cross(Vector3.up, forward).normalized * rotationCorrectionStrength *
                                       verticalDot;
            rb.AddTorque(correctionTorque * Time.fixedDeltaTime, ForceMode.Impulse);
        }
        else
        {
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