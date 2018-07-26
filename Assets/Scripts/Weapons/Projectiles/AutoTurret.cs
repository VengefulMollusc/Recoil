using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class AutoTurret : MonoBehaviour
{
    public float launchForce;
    public float bounceDistance;
    public Weapon turretWeapon;
    public AutoTargeter targeter;

    private Rigidbody rb;
    private HealthController health;
    private TrailRenderer trail;

    private bool deployed;
    private Vector3 deployedPosition;
    private bool despawning;
    private bool hasTarget;

    public void Launch(Vector3 origin, Vector3 direction, string newOwner)
    {
        turretWeapon.SetOwnerString(newOwner);

        deployed = false;
        despawning = false;
        hasTarget = false;

        transform.position = origin;
        //transform.rotation = Quaternion.LookRotation(direction);

        if (health == null)
            health = GetComponent<HealthController>();

        health.ResetHealth();

        if (trail == null)
            trail = GetComponent<TrailRenderer>();

        trail.Clear();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.drag = 0f;
        rb.useGravity = true;
        rb.isKinematic = false;

        gameObject.SetActive(true);

        rb.AddForce(direction * launchForce, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision col)
    {
        deployedPosition = col.contacts[0].point + (col.contacts[0].normal * bounceDistance);
        rb.drag = 1f;
        rb.useGravity = false;
        deployed = true;
    }

    void FixedUpdate()
    {
        if (!gameObject.activeSelf || despawning)
            return;

        if (health.IsDead())
        {
            TriggerDespawn();
            return;
        }

        if (!deployed)
            return;

        Vector3 position = transform.position;
        if (position != deployedPosition)
        {
            Vector3 diff = deployedPosition - position;
            rb.AddForce(diff * Time.fixedDeltaTime, ForceMode.Impulse);
        }

        bool targeterHasTarget = targeter.HasTarget();
        if (targeterHasTarget != hasTarget)
        {
            hasTarget = targeterHasTarget;
            turretWeapon.FireWeapon(hasTarget);
        }
    }

    public bool IsOwner(string other)
    {
        string owner = turretWeapon.GetOwnerString();
        if (owner == null)
            return false;
        return other.Equals(owner);
    }

    public void TriggerDespawn()
    {
        if (despawning)
            return;

        StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {
        rb.isKinematic = true;
        hasTarget = false;
        despawning = true;
        yield return new WaitForSeconds(0.2f);
        GameObjectPoolController.Enqueue(GetComponent<Poolable>());
    }
}
