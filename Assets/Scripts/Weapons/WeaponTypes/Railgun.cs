using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Railgun : Weapon
{
    private const float range = 200f;
    private const float fireRate = 0.25f;
    private const float damage = 10f;

    private bool firing;
    private bool firingSequenceActive;

    private Vector3 hitPosition;

    public override void FireWeapon(bool pressed)
    {
        firing = pressed;
        if (pressed && !firingSequenceActive)
        {
            StartCoroutine(FiringSequenceCoroutine());
        }
    }

    /*
     * Handles timing of weapon firing to not exceed firerate
     */
    private IEnumerator FiringSequenceCoroutine()
    {
        firingSequenceActive = true;

        while (firing)
        {
            Fire();
            yield return new WaitForSeconds(fireRate);
        }

        firingSequenceActive = false;
    }

    /*
     * Raycast to maximum range and damage any damageable objects hit
     */
    private void Fire()
    {
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        RaycastHit hitInfo;

        hitPosition = origin + (direction * range);

        if (Physics.Raycast(origin, direction, out hitInfo, range))
        {
            hitPosition = hitInfo.point;

            HealthController healthController = hitInfo.collider.gameObject.GetComponent<HealthController>();
            if (healthController != null)
            {
                healthController.Damage(damage);
            }
        }
    }

    /*
     * Temporary VFX
     */
    void OnDrawGizmos()
    {
        if (firing)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, hitPosition);
        }
    }
}
