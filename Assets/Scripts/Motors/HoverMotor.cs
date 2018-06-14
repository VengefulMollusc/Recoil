using System.Collections;
using System.Collections.Generic;
using System.Security.Principal;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : MonoBehaviour
{
    [Header("General")]
    [SerializeField]
    private float verticalDrag = 0.01f;
    [SerializeField] private float gravityForce = 10f;

    [Header("Movement")]
    [SerializeField]
    private float maxSpeed = 20f;
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float leanStrength = 0.5f;

    [Header("Turning")]
    [SerializeField]
    private float turnSpeed = 4f;
    [SerializeField] private float baseAngularDrag = 4f;
    [SerializeField] private float turningAngularDrag = 2f;

    [Header("Boost")]
    [SerializeField]
    private float boostForceMultiplier = 2f;
    [SerializeField] private float boostTime = 1.5f;
    [SerializeField] private float boostRechargeTime = 3f;
    [SerializeField] private float boostRechargeDelay = 1.5f;

    [Header("Hover")]
    [SerializeField]
    private float hoverHeight = 15f;
    [SerializeField] private float minHoverHeight = 10f;
    [SerializeField] private float maxHoverHeight = 20f;
    [SerializeField] private float heightChangeForce = 3f;
    [SerializeField] private float heightChangeRate = 10f;
    [SerializeField] private float hoverForce = 30f;

    [Header("Gyro")]
    [SerializeField]
    private float gyroCorrectionStrength = 6f;
    [SerializeField] private float rotationLimit = 0.6f;
    [SerializeField] private float rotationCorrectionStrength = 4f;

    private Rigidbody rb;
    private Vector2 moveInputVector;
    private Vector2 turnInputVector;
    private Vector3 gravityVector;

    private bool boosting;
    private float boostState;

    // Cached direction variables
    private Vector3 up;
    private Vector3 forward;
    private Vector3 forwardFlat;
    private Vector3 right;
    private Vector3 rightFlat;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityVector = Vector3.down * gravityForce;

        // sqr maxspeed for cheaper comparisons later
        maxSpeed *= maxSpeed;

        UpdateDirectionVariables();
    }

    void UpdateDirectionVariables()
    {
        // update cached transform variables
        forward = transform.forward;
        up = transform.up;
        right = transform.right;

        // Get movement axes relative to global axes rather than local vertical look direction
        forwardFlat = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, forwardFlat);
        rightFlat = rot * Vector3.right;
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

    public void Boost(bool isBoosting)
    {
        if (boosting && !isBoosting)
            StartCoroutine(RechargeBoost());
        else
            boosting = isBoosting && boostState < boostTime;
    }

    void FixedUpdate()
    {
        UpdateDirectionVariables();

        // Apply Gravity
        rb.AddForce(gravityVector * Time.fixedDeltaTime, ForceMode.Impulse);

        // Movement
        if (boosting)
            ApplyBoost();
        else
            ApplyMovementForce();

        // Turning
        ApplyTurningForce();
        DampenTurning();

        // Hover force and gyro correction
        ApplyHoverForce();
        GyroCorrection();

        Debug.Log(boostState);
    }

    void ApplyMovementForce()
    {
        // Apply force to move tank
        if (moveInputVector == Vector2.zero)
            return;

        Vector3 inputForce = (forwardFlat * moveInputVector.y)
            + (rightFlat * moveInputVector.x);
        rb.AddForce(inputForce * moveForce * Time.fixedDeltaTime, ForceMode.Impulse);

        // lean slightly to match left/right movement
        Vector3 leanTorque = forwardFlat * -moveInputVector.x * leanStrength;
        rb.AddTorque(leanTorque * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void ApplyBoost()
    {
        // Add boost force
        rb.AddForce(forward * moveForce * boostForceMultiplier * Time.fixedDeltaTime, ForceMode.Impulse);

        // track boost state and begin recharge if limit is hit
        boostState += Time.fixedDeltaTime;
        if (boostState >= boostTime)
        {
            boostState = boostTime;
            StartCoroutine(RechargeBoost());
        }
    }

    // Recharges boost value after a delay
    private IEnumerator RechargeBoost()
    {
        boosting = false;

        // wait for the recharge delay
        yield return new WaitForSeconds(boostRechargeDelay);

        // calculate recharge rate based on time
        float rechargeRate = boostTime / boostRechargeTime;

        // Recharge boost
        while (boostState > 0f && !boosting)
        {
            boostState -= Time.deltaTime * rechargeRate;
            yield return 0;
        }

        if (!boosting)
            boostState = 0f;
    }

    void ApplyTurningForce()
    {
        // TODO: tweak this for smooth controller/mouse input
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
            float force = Utilities.MapValues(distanceToGround, hoverHeight, 0f, 0f, boosting ? hoverForce * boostForceMultiplier : hoverForce);
            rb.AddForce(Vector3.up * force * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        // apply vertical momentum drag
        Vector3 velocity = rb.velocity;
        velocity.y *= 1f - verticalDrag;
        rb.velocity = velocity;
    }

    void GyroCorrection()
    {
        // check against vertical rotation limit
        float verticalDot = Vector3.Dot(forward, Vector3.up);
        if (Mathf.Abs(verticalDot) > rotationLimit)
        {
            // Vertical rotation correction
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