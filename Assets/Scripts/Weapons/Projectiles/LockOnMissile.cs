using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SimpleHealthController))]
public class LockOnMissile : MonoBehaviour
{
    [Header("Damage")]
    public float damage;
    public float explosionRadius;
    public float explosionDamage;
    public float explosionForce;
    public LayerMask explosionLayerMask;

    [Header("Movement")]
    public float acceleration;
    public float lifeSpan;
    public float launchTime;
    public float launchForce;
    public float initialGravity;
    public float homingStrength;

    [Header("Particles")]
    public ParticleSystem trailParticles;
    public ParticleSystem explosionParticles;

    private Transform target;
    private Rigidbody rb;
    private HealthController health;
    private TrailRenderer trail;

    private float timer;

    public void Launch(Vector3 position, Vector3 facingDirection, Vector3 launchDirection, Transform target, Vector3 parentVelocity)
    {
        transform.position = position;
        transform.rotation = Quaternion.LookRotation(facingDirection, launchDirection);

        this.target = target;
        timer = 0f;

        gameObject.SetActive(true);

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (health == null)
            health = GetComponent<SimpleHealthController>();

        health.ResetHealth();

        if (trail == null)
            trail = GetComponent<TrailRenderer>();

        rb.isKinematic = false;
        rb.detectCollisions = true;
        rb.velocity = parentVelocity;
        rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);

        // start trail effects
        trailParticles.Play();
        trail.Clear();
    }

    void FixedUpdate()
    {
        if (!gameObject.activeSelf || rb.isKinematic)
            return;

        timer += Time.fixedDeltaTime;

        if (health.IsDead() || timer > lifeSpan)
        {
            Explode();
            return;
        }

        float launchRatio = 1f;

        if (timer < launchTime)
        {
            launchRatio = Mathf.Clamp01(timer / launchTime);
            float falloff = 1 - launchRatio;
            ApplyGravity(falloff);
        }

        // Apply rocket force
        Vector3 forward = transform.forward;
        Vector3 force = forward * acceleration * 10f;

        //if (timer > launchTime)
        //{
        // Apply direction correction force
        Vector3 velocity = rb.velocity;
        Vector3 projected = Vector3.Project(rb.velocity, forward);

        if (Vector3.Dot(velocity, projected) > 0f)
        {
            Vector3 diff = projected - velocity;
            force += diff * acceleration * launchRatio;
        }

        if (target != null)
        {
            //transform.LookAt(target.position);

            Vector3 toTarget = (target.position - transform.position).normalized;
            Vector3 newFacing = Vector3.RotateTowards(forward, toTarget, homingStrength * Time.fixedDeltaTime * launchRatio, 0f);
            rb.rotation = Quaternion.LookRotation(newFacing);

            //Debug.DrawLine(transform.position, target.position);
        }
        //}

        // Apply forces
        rb.AddForce(force * Time.fixedDeltaTime, ForceMode.Acceleration);
    }

    private void ApplyGravity(float strength)
    {
        float gravityForce = initialGravity * Time.fixedDeltaTime * 10f * strength;
        rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision col)
    {
        // damage stuff
        HealthController healthController = col.collider.GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.Damage(damage);
        }

        // TODO: figure out if impact force needed
        //// apply impact force
        //Rigidbody rb = col.collider.GetComponent<Rigidbody>();
        //if (rb != null)
        //{
        //    Vector3 force = transform.forward * impactForce;
        //    rb.AddForceAtPosition(force, transform.position, ForceMode.Impulse);
        //}

        Explode();
    }

    void Explode()
    {
        rb.isKinematic = true;
        rb.detectCollisions = false;

        // TODO: explosion particle effects here
        trailParticles.Stop();

        if (explosionParticles != null)
            explosionParticles.Play();

        // Apply explosion damage and force
        Vector3 position = transform.position;
        Collider[] hitColliders = Physics.OverlapSphere(position, explosionRadius, explosionLayerMask, QueryTriggerInteraction.Ignore);
        foreach (Collider col in hitColliders)
        {
            // damage health
            HealthController health = col.GetComponent<HealthController>();
            if (health != null)
            {
                health.Damage(explosionDamage);
            }

            // add explosion force
            Rigidbody rigidbody = col.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.AddExplosionForce(explosionForce, position, explosionRadius, 2f, ForceMode.Impulse);
                //Vector3 force = (col.transform.position - position).normalized * explosionForce;
                //rigidbody.AddForce(force, ForceMode.Impulse);
            }
        }

        StartCoroutine(Despawn());
    }

    private IEnumerator Despawn()
    {
        yield return new WaitForSeconds(Mathf.Max(trail.time, explosionParticles.main.duration));
        GameObjectPoolController.Enqueue(GetComponent<Poolable>());
    }
}
