using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Poolable))]
public class Bullet : MonoBehaviour
{
    public float lifeSpan = 2f;
    public float damage = 10f;
    public float distancePerSecond = 100f;

    private Poolable poolable;
    private Vector3 movementVector;
    private TrailRenderer trailRenderer;

    private bool despawning;

    private float timer;

    public void Launch(Vector3 position, Vector3 direction)
    {
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
        if (Physics.Raycast(transform.position, movementVector, out hitInfo, movementVector.magnitude * Time.deltaTime))
        {
            float distance = hitInfo.distance;
            transform.position += movementVector.normalized * distance * Time.deltaTime;
            Collide(hitInfo.collider);
            return;
        }

        transform.position += movementVector * Time.deltaTime;
        timer += Time.deltaTime;

        if (timer >= lifeSpan)
        {
            StartCoroutine(Despawn());
        }
    }

    void Collide(Collider col)
    {
        HealthController healthController = col.gameObject.GetComponent<HealthController>();
        if (healthController != null)
        {
            healthController.Damage(damage);
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
