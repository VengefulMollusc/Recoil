using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SimpleHealthController))]
public class LockOnMissile : MonoBehaviour
{
    public float damage;
    public float acceleration;
    public float lifeSpan;
    public float ignitionTime;
    public float launchVelocity;
    public float initialGravity;
    public float homingStrength;

    private Rigidbody rb;
    private Transform target;
    private HealthController health;

    private bool launching;
    private float timer;
    private float ignitionTimer;

    public void Launch(Vector3 position, Vector3 facingDirection, Vector3 launchDirection, Transform target, Vector3 parentVelocity)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(facingDirection, launchDirection);

        this.target = target;
        launching = true;
        timer = 0f;

        gameObject.SetActive(true);

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (health == null)
            health = GetComponent<SimpleHealthController>();

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.velocity = (launchDirection * launchVelocity) + parentVelocity;
    }

    void FixedUpdate()
    {
        if (!gameObject.activeSelf)
            return;

        timer += Time.fixedDeltaTime;

        if (health.isDead() || timer > lifeSpan)
        {
            Explode();
            return;
        }

        if (launching)
        {
            ApplyGravity(1f);

            if (rb.velocity.y <= 0f)
                Ignite();

            return;
        }

        // Apply rocket force
        Vector3 forward = transform.forward;
        //float accelerationForce = acceleration * Time.fixedDeltaTime * 10f;
        //rb.AddForce(forward * accelerationForce, ForceMode.Acceleration);

        Vector3 force = forward * acceleration * 10f;

        if (ignitionTimer < ignitionTime)
        {
            float falloff = 1 - (ignitionTimer / ignitionTime);
            ApplyGravity(falloff);
            ignitionTimer += Time.fixedDeltaTime;
        }

        // Apply direction correction force
        Vector3 velocity = rb.velocity;
        Vector3 projected = Vector3.Project(rb.velocity, forward);
        if (Vector3.Dot(velocity, projected) > 0f)
        {
            Vector3 diff = projected - velocity;
            force += diff * acceleration;
        }

        if (target != null)
        {
            //transform.LookAt(target.position);

            Vector3 toTarget = (target.position - transform.position).normalized;
            Vector3 newFacing = Vector3.RotateTowards(forward, toTarget, homingStrength * Time.fixedDeltaTime, 0f);
            rb.rotation = Quaternion.LookRotation(newFacing);

            Debug.DrawLine(transform.position, target.position);
        }

        // Apply forces
        rb.AddForce(force * Time.fixedDeltaTime, ForceMode.Acceleration);

        //// do tracking stuff here
        //Vector3 forward = transform.forward;
        //float accelerationForce = acceleration * Time.fixedDeltaTime / Time.timeScale;
        //rb.AddForce(forward * accelerationForce, ForceMode.Acceleration);

        //if (target != null)
        //{
        //    Vector3 toTarget = (target.position - transform.position).normalized;
        //    Vector3 newFacing = Vector3.RotateTowards(forward, toTarget, homingStrength * Time.fixedDeltaTime, 0f);
        //    Quaternion rot = Quaternion.FromToRotation(forward, newFacing);
        //    rb.rotation *= rot;

        //    Debug.DrawLine(transform.position, target.position);
        //}
    }

    private void ApplyGravity(float strength)
    {
        float gravityForce = initialGravity * Time.fixedDeltaTime * 10f * strength;
        rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
    }

    void Ignite()
    {
        // activate rocket trail particle effect here
        launching = false;
        ignitionTimer = 0f;
    }

    void OnCollisionEnter(Collision col)
    {
        if (launching)
            return;

        // damage stuff
        HealthController healthController = col.gameObject.GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.Damage(damage);
        }

        Explode();
    }

    void Explode()
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;
        // explode damage and particle effects
        GameObjectPoolController.Enqueue(GetComponent<Poolable>());
    }
}
