using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Railgun : Weapon
{
    public float range = 200f;
    public float beamThickness = 1f;
    public float chargeTime = 1.5f;
    public float damage = 10f;
    public float impactForce = 10f;
    public Vector3 firingPoint;
    public LayerMask layerMask;

    private bool charging;
    private float charge;
    private Rigidbody parentRb;

    public override void FireWeapon(bool pressed)
    {
        charging = pressed;
        if (charging)
        {
            StartCoroutine(ChargingSequence());
        }
    }

    /*
     * Handles charging and firing of weapon
     */
    private IEnumerator ChargingSequence()
    {
        charge = 0f;
        while (charging)
        {
            charge += Time.deltaTime;
            yield return 0;
        }

        if (charge >= chargeTime)
            Fire();
    }

    /*
     * Raycast to maximum range and damage any damageable objects hit
     */
    private void Fire()
    {
        Vector3 origin = transform.TransformPoint(firingPoint);
        Vector3 direction = transform.forward;

        // Apply recoil force
        if (parentRb == null)
            parentRb = GetComponentInParent<Rigidbody>();
        parentRb.AddForceAtPosition(-direction * impactForce, origin, ForceMode.Impulse);

        RaycastHit[] hits = Physics.SphereCastAll(origin, beamThickness, direction, range, layerMask);

        float endDist = range;

        // loop through once to find terrain distance
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.gameObject.layer == 11 && hit.distance < endDist)
            {
                endDist = hit.distance;
            }
        }

        foreach (RaycastHit hit in hits)
        {
            if (hit.distance > endDist)
            {
                continue;
            }

            HealthController healthController = hit.collider.GetComponent<HealthController>();
            if (healthController != null)
            {
                healthController.Damage(damage);
            }

            Rigidbody impactRb = hit.collider.GetComponent<Rigidbody>();
            if (impactRb != null)
            {
                impactRb.AddForceAtPosition(direction * impactForce, hit.point, ForceMode.Impulse);
            }
        }

        ActivateFiringEffects(origin, direction, endDist);
    }

    private void ActivateFiringEffects(Vector3 origin, Vector3 direction, float distance)
    {
        bool hitTerrain = distance < range; // if true, use explosion effect at end
        Vector3 endPoint = origin + direction * distance;

        // TODO: activate visual effects from origin to endPoint
    }
}
