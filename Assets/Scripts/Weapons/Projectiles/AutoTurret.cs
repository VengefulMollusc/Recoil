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

    private bool deployed;
    private Vector3 deployedPosition;
    private bool despawning;
    private bool hasTarget;

    public void Launch(Vector3 origin, Vector3 direction)
    {
        deployed = false;
        despawning = false;

        transform.position = origin;
        transform.rotation = Quaternion.LookRotation(direction);

        if (health == null)
            health = GetComponent<HealthController>();

        health.ResetHealth();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        rb.drag = 0f;

        gameObject.SetActive(true);

        rb.velocity = Vector3.zero;
        rb.AddForce(direction * launchForce, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision col)
    {
        deployedPosition = col.contacts[0].point + (col.contacts[0].normal * bounceDistance);
        rb.drag = 1f;
        deployed = true;
        hasTarget = false;
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

    public void TriggerDespawn()
    {
        StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {
        hasTarget = false;
        despawning = true;
        deployed = false;
        yield return new WaitForSeconds(0.5f);
        GameObjectPoolController.Enqueue(GetComponent<Poolable>());
    }
}
