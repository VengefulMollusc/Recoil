using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private float verticalDrag = 0.01f;
    [SerializeField] private float gravityForce = 10f;

    [Header("Movement")]
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float leanStrength = 0.5f;

    [Header("Turning")]
    [SerializeField] private float turnSpeed = 4f;
    [SerializeField] private float baseAngularDrag = 4f;
    [SerializeField] private float turningAngularDrag = 2f;

    [Header("Boost")]
    [SerializeField] private float boostForceMultiplier = 2f;
    [SerializeField] private float boostTime = 1.5f;
    [SerializeField] private float boostRechargeTime = 3f;
    [SerializeField] private float boostRechargeDelay = 1.5f;

    [Header("Hover")]
    [SerializeField] private float hoverHeight = 15f;
    [SerializeField] private float minHoverHeight = 10f;
    [SerializeField] private float maxHoverHeight = 20f;
    [SerializeField] private float heightChangeForce = 3f;
    [SerializeField] private float heightChangeRate = 10f;
    [SerializeField] private float hoverForce = 30f;
    [SerializeField] private LayerMask raycastMask;
    public List<Vector3> raycastDirections;
    public float rayCastHeightModifier = 2f;
    public float rayCastHorizontalLengthModifier = 2.5f;

    [Header("Gyro")]
    [SerializeField] private float gyroRotationLimit = 0.7f;
    [SerializeField] private float gyroCorrectionStrength = 6f;
    [SerializeField] private float rotationLimit = 0.6f;
    [SerializeField] private float rotationCorrectionStrength = 4f;

    private Rigidbody rb;
    private Vector2 moveInputVector;
    private Vector2 turnInputVector;
    private Vector3 gravityVector;

    private bool boosting;
    private float boostState;

    private bool useBumperForce;
    private float bumperColliderRadius;

    // Cached variables
    private Vector3 position;
    private Vector3 up;
    private Vector3 forward;
    private Vector3 forwardFlat;
    private Vector3 right;
    private Vector3 rightFlat;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityVector = Vector3.down * gravityForce;

        bumperColliderRadius = GetComponent<SphereCollider>().radius;

        UpdateVariables();
        ProcessHoverRays();
    }

    /*
     * Normalise hover raycasts.
     *
     * TODO: when using fixed hover height, verticalSpread can be baked in here
     */
    void ProcessHoverRays()
    {
        foreach (Vector3 ray in raycastDirections)
        {
            ray.Normalize();
        }
    }

    /*
     * Caches cardinal and flat direction variables for use by logic functions
     * run at the start of each update
     */
    void UpdateVariables()
    {
        // update cached transform variables
        position = transform.position;
        forward = transform.forward;
        up = transform.up;
        right = transform.right;

        // Get movement axes relative to global axes rather than local vertical look direction
        forwardFlat = Vector3.ProjectOnPlane(forward, Vector3.up).normalized;
        Quaternion rot = Quaternion.FromToRotation(Vector3.forward, forwardFlat);
        rightFlat = rot * Vector3.right;
    }

    /*
     * Handle movement input
     */
    public void Move(float x, float y)
    {
        moveInputVector = new Vector2(x, y);
        if (moveInputVector.sqrMagnitude > 1f)
            moveInputVector.Normalize();
    }

    /*
     * Handle turning input
     *
     * TODO: Rebuild turning/aiming to add second level of precise aiming.
     * TODO: Use joystick for basic turning, then floating reticle for precise aiming.
     * TODO: reticle controlled by eg: switch gyro.
     * TODO: act roughly like killzone 3 psmove aiming
     */
    public void MoveCamera(float x, float y)
    {
        // TODO: tweak this for smooth controller/mouse input
        turnInputVector = new Vector2(x, y);
        if (turnInputVector.sqrMagnitude > 1f)
            turnInputVector.Normalize();
    }

    /*
     * Handle hover height adjustment input
     *
     * TODO: refactor to just use constant hover height
     */
    public void ChangeHeight(float change)
    {
        // Change hover height between limits
        hoverHeight = Mathf.Clamp(hoverHeight + (change * heightChangeRate * Time.deltaTime),
            minHoverHeight, maxHoverHeight);

        // boost hover force in direction of change
        rb.AddForce(Vector3.up * change * heightChangeForce * Time.deltaTime, ForceMode.Impulse);
    }

    /*
     * Handle boost input
     */
    public void Boost(bool isBoosting)
    {
        if (boosting && !isBoosting)
            StartCoroutine(RechargeBoost());
        else
            boosting = isBoosting && boostState < boostTime;
    }

    void FixedUpdate()
    {
        // Update cardinal/flat direction variables etc
        UpdateVariables();

        // Apply Gravity
        rb.AddForce(gravityVector * Time.fixedDeltaTime, ForceMode.VelocityChange);

        // Movement
        ApplyMovementForce();

        // Turning
        ApplyTurningForce();
        DampenTurning();

        // Hover force, bumper forces and gyro correction
        ApplyHoverForce();
        ApplyBumperForce();
        GyroCorrection();

        // reset variables
        useBumperForce = false;
    }

    /*
     * Apply movement force based on input
     */
    void ApplyMovementForce()
    {
        if (moveInputVector == Vector2.zero)
            return;

        // Apply force to move tank
        if (boosting)
        {
            // Apply boosting movement force
            // Uses actual forward/right vectors rather than flat vectors to allow more directional control 
            Vector3 boostForce = (forward * moveInputVector.y) + (right * moveInputVector.x);
            rb.AddForce(boostForce.normalized * moveForce * boostForceMultiplier * Time.fixedDeltaTime, ForceMode.Impulse);

            // track boost state and begin recharge if limit is hit
            boostState += Time.fixedDeltaTime;
            if (boostState >= boostTime)
            {
                boostState = boostTime;
                StartCoroutine(RechargeBoost());
            }
        }
        else
        {
            // Not boosting: apply default movement force
            Vector3 movementForce = (forwardFlat * moveInputVector.y)
                + (rightFlat * moveInputVector.x);
            rb.AddForce(movementForce * moveForce * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        // lean slightly to match left/right movement
        // TODO: detach visuals slightly from camera, allow leaning forward/back as well
        Vector3 leanTorque = forwardFlat * -moveInputVector.x * leanStrength;
        rb.AddTorque(leanTorque * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    /*
     * Coroutine to recharge boost
     *
     * Stops when fully recharged or boost input is activated again
     */
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

    /*
     * Apply turning torque based on input
     */
    void ApplyTurningForce()
    {
        if (turnInputVector == Vector2.zero)
            return;

        // Apply torque to rotate facing direction
        Vector3 torque = (up * turnInputVector.x * turnSpeed) + (right * -turnInputVector.y * turnSpeed);

        rb.AddTorque(torque * Time.fixedDeltaTime, ForceMode.Impulse);

    }

    /*
     * Switch angular drag value based on whether receiving turn input
     * (no input = higher drag for more accurate stopping)
     */
    void DampenTurning()
    {
        // Apply angular drag value based on input state
        rb.angularDrag = (turnInputVector == Vector2.zero) ? baseAngularDrag : turningAngularDrag;
    }

    /*
     * Perform raycasts underneath and apply hover force based on the closest hit
     */
    void ApplyHoverForce()
    {
        // raycast and apply hover force
        float maxHoverForce = 0f;
        Vector3 origin = position + (Vector3.up * rayCastHeightModifier);
        foreach (Vector3 ray in raycastDirections)
        {
            RaycastHit hitInfo;
            float rayLength = CalculateHoverRayLength(ray);
            if (Physics.Raycast(origin, ray, out hitInfo, rayLength, raycastMask))
            {
                if (hitInfo.distance > rayCastHeightModifier) // this check to make sure raycasts ending above player collider dont trigger hover
                {
                    float force = (1 - (hitInfo.distance - rayCastHeightModifier) / (rayLength - rayCastHeightModifier)) * hoverForce;
                    if (force > maxHoverForce)
                        maxHoverForce = force;
                }
            }
        }
        if (maxHoverForce > 0f)
        {
            rb.AddForce(Vector3.up * maxHoverForce * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        // apply vertical momentum drag
        Vector3 velocity = rb.velocity;
        velocity.y *= 1f - verticalDrag;
        rb.velocity = velocity;
    }

    /*
     * Calculates the length of a given hover raycast based on angles to vertical axis and velocity direction
     */
    public float CalculateHoverRayLength(Vector3 ray)
    {
        Vector3 rayNormalised = ray.normalized;

        // extend raycast length depending on angle from vertical
        // TODO: NOTE: if hoverHeight is ever made constant, this can be baked into the rays at Start()
        float verticalDot = (Vector3.Dot(Vector3.up, rayNormalised) + 1) * 0.5f;
        float verticalSpread = verticalDot * hoverHeight * rayCastHorizontalLengthModifier;

        // this line so inspector script doesnt trip over missing rb before start
        if (rb == null)
            return hoverHeight + verticalSpread;

        // extend raycast length depending on angle to movement direction
        float movementSpread = 0f;
        Vector3 movementVector = rb.velocity;
        if (movementVector != Vector3.zero && Vector3.Angle(movementVector, ray) < 60f)
        {
            float movementDot = Vector3.Dot(movementVector * 0.01f, rayNormalised);
            if (movementDot > 0f)
                movementSpread = movementDot * hoverHeight * rayCastHorizontalLengthModifier;
        }

        return hoverHeight + verticalSpread + movementSpread;
    }

    /*
     * Shunts tank away from close surfaces in cardinal directions
     */
    void ApplyBumperForce()
    {
        if (!useBumperForce)
            return;

        // if trigger collider is colliding
        List<Vector3> rays = new List<Vector3>()
        {
            forward,
            -forward,
            right,
            -right,
            up,
            -up
        };
        Vector3 bumperForce = Vector3.zero;
        // raycast in relative cardinal directions and average force
        foreach (Vector3 ray in rays)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(position, ray, out hitInfo, bumperColliderRadius, raycastMask))
            {
                float force = (1 - hitInfo.distance / bumperColliderRadius) * hoverForce;
                bumperForce -= ray * force * 2f;
            }
        }
        if (bumperForce != Vector3.zero)
            rb.AddForce(bumperForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    /*
     * Apply torque to level tank
     */
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

        // Correct gyro
        if (Mathf.Abs(verticalDot) < gyroRotationLimit)
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

    /*
     * Trigger collider used to trigger bumper raycasts
     */
    void OnTriggerStay(Collider col)
    {
        if (!col.isTrigger)
        {
            useBumperForce = true;
        }
    }
}