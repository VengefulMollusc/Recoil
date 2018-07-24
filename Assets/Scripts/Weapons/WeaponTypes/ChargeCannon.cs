using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChargeCannon : Weapon
{
    public float range = 200f;
    public float beamRadius = 1f;
    public float minChargeTime = 0.25f;
    public float maxChargeTime = 1.5f;
    public float damage = 10f;
    public float impactForce = 10f;
    public FiringPoint firingPoint;
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
        while (charging && charge <= maxChargeTime + 0.5f)
        {
            charge += Time.deltaTime;
            yield return null;
        }

        Fire();
    }

    /*
     * Spherecast to maximum range and damage any damageable objects hit
     */
    private void Fire()
    {
        if (charge < minChargeTime)
            return;

        float chargeLevel = Mathf.Clamp01(charge / maxChargeTime);
        
        Vector3 origin = firingPoint.transform.position;
        Vector3 direction = firingPoint.transform.forward;

        RaycastHit[] hits = Physics.SphereCastAll(origin, beamRadius, direction, range, layerMask);

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
            if (hit.distance > endDist || transform.IsChildOf(hit.collider.transform))
            {
                continue;
            }

            HealthController healthController = hit.collider.GetComponent<HealthController>();
            if (healthController != null)
            {
                healthController.Damage(damage * chargeLevel);
            }

            Rigidbody impactRb = hit.collider.GetComponent<Rigidbody>();
            if (impactRb != null)
            {
                impactRb.AddForceAtPosition(direction * impactForce * chargeLevel, hit.point, ForceMode.Impulse);
                impactRb.AddForce(direction * impactForce * chargeLevel, ForceMode.Impulse);
            }
        }

        ActivateFiringEffects(origin, direction, endDist, chargeLevel);
    }

    private void ActivateFiringEffects(Vector3 origin, Vector3 direction, float distance, float chargeLevel)
    {
        // Apply recoil force
        if (parentRb == null)
            parentRb = GetComponentInParent<Rigidbody>();
        parentRb.AddForceAtPosition(-direction * impactForce * chargeLevel, origin, ForceMode.Impulse);
        parentRb.AddForce(-direction * impactForce * chargeLevel, ForceMode.Impulse);

        firingPoint.Fire();

        bool hitTerrain = distance < range; // if true, use explosion effect at end
        Vector3 endPoint = origin + direction * distance;

        Debug.Log("Fired: " + (chargeLevel * 100f) + "%");
        // TODO: activate visual effects from origin to endPoint
    }
}
