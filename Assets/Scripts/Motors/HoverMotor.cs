using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : Motor
{
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float hoverHeight = 5f;
    [SerializeField] private float hoverForce = 20f;
    [SerializeField] private float gravityForce = 10f;
    [SerializeField] private float dampenForceFactor = 0.4f;

    private Rigidbody rb;
    private Vector2 inputVector;
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

        ApplyInputForce();
        DampenMovement();

        ApplyHoverForce();
    }

    void ApplyInputForce()
    {
        Vector3 inputForce = (transform.forward * inputVector.y) 
            + (transform.right * inputVector.x);
        rb.AddForce(inputForce * moveForce * Time.fixedDeltaTime, ForceMode.Impulse);
    }

    void DampenMovement()
    {
        //if (inputVector != Vector2.zero)
        //    return;

        Vector3 flatVel = Vector3.ProjectOnPlane(rb.velocity, Vector3.up);
        float dampenForce = Utilities.MapValues(flatVel.sqrMagnitude, 0f, maxSpeed, 0.1f, moveForce * dampenForceFactor);
        rb.AddForce(-flatVel.normalized * dampenForce * Time.deltaTime, ForceMode.Impulse);
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
        inputVector = new Vector2(x, y);
        if (inputVector.sqrMagnitude > 1f)
            inputVector.Normalize();
    }
}