using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverMotor : Motor
{
    [SerializeField] private float moveForce = 10f;
    [SerializeField] private float hoverHeight = 20f;
    [SerializeField] private float hoverForce = 10f;
    [SerializeField] private float gravityForce = 5f;

    private Rigidbody rb;
    private Vector2 inputVector;
    private Vector3 gravityVector;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gravityVector = Vector3.down * gravityForce;
    }

    void FixedUpdate()
    {
        // Apply Gravity
        rb.AddForce(gravityVector, ForceMode.Force);

        ApplyInputForce();
        ApplyHoverForce();
    }

    void ApplyInputForce()
    {
        Vector3 inputForce = (transform.forward * inputVector.x) + (transform.right * inputVector.y) * moveForce;
        rb.AddForce(inputForce, ForceMode.Force);
    }

    void ApplyHoverForce()
    {
        // raycast down and apply hover force
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo, hoverHeight))
        {
            float distanceToGround = hitInfo.distance;
            float force = Utilities.MapValues(distanceToGround, hoverHeight, 0f, 0f, hoverForce);
            rb.AddForce(Vector3.up * force, ForceMode.Force);
        }
    }

    public override void Move(float x, float y)
    {
        inputVector = new Vector2(x, y);
        if (inputVector.sqrMagnitude > 1f)
            inputVector.Normalize();
    }
}