using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class Bullet : MonoBehaviour
{
    public float lifeSpan = 2f;
    public float damage = 10f;
    public float impactForce;
    public float distancePerSecond = 100f;

    public LayerMask layerMask;

    private Poolable poolable;
    private Vector3 movementVector;
    private TrailRenderer trailRenderer;

    private bool despawning;

    private GameObject sourceObject;

    private float timer;

    public void Launch(Vector3 position, Vector3 direction, GameObject source)
    {
        sourceObject = source;
        transform.position = position;
        movementVector = direction.normalized * distancePerSecond;
        timer = 0f;
        gameObject.SetActive(true);
        despawning = false;

        if (trailRenderer == null)
        {
            trailRenderer = GetComponent<TrailRenderer>();
        }
        trailRenderer.Clear();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameObject.activeSelf || despawning)
            return;

        // raycast ahead for collisions
        RaycastHit hitInfo;
        if (Physics.Raycast(transform.position, movementVector, out hitInfo, movementVector.magnitude * Time.deltaTime, layerMask, QueryTriggerInteraction.Ignore))
        {
            if (hitInfo.collider.gameObject != sourceObject)
            {
                float distance = hitInfo.distance;
                transform.position += movementVector.normalized * distance * Time.deltaTime;
                Collide(hitInfo);
                return;
            }
        }

        transform.position += movementVector * Time.deltaTime;
        timer += Time.deltaTime;

        if (timer >= lifeSpan)
        {
            StartCoroutine(Despawn());
        }
    }

    void Collide(RaycastHit hitInfo)
    {
        // trigger impact particle effect

        Collider col = hitInfo.collider;

        // damage object
        HealthController healthController = col.GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.Damage(damage);
        }

        // apply impact force
        Rigidbody rb = col.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 force = movementVector.normalized * impactForce;
            rb.AddForceAtPosition(force, hitInfo.point, ForceMode.Impulse);
        }

        StartCoroutine(Despawn());
    }

    IEnumerator Despawn()
    {
        despawning = true;
        yield return new WaitForSeconds(trailRenderer.time);
        GameObjectPoolController.Enqueue(GetComponent<Poolable>());
    }
}
