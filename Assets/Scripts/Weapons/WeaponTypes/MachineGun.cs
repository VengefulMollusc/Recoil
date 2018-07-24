using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : Weapon
{
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    public float spread = 0.02f;
    public List<FiringPoint> firingPoints;

    private bool firing;
    private bool firingSequenceActive;

    private string poolableBulletKey;
    private int firingPointIndex;

    private Rigidbody parentRb;

    void Start()
    {
        poolableBulletKey = bulletPrefab.GetComponent<Poolable>().key;
        int poolableCount = (int) (bulletPrefab.GetComponent<Bullet>().lifeSpan / fireRate);
        GameObjectPoolController.AddEntry(poolableBulletKey, bulletPrefab, poolableCount, poolableCount * ScenePlayerController.GetPlayerCount());

        if (firingPoints.Count <= 0)
            Debug.LogError("No firingPoints defined");

        parentRb = GetComponentInParent<Rigidbody>();
    }

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
     * Get a bullet object from the pool controller
     */
    private void Fire()
    {
        Transform firingPoint = firingPoints[firingPointIndex].transform;
        Vector3 origin = firingPoint.position;
        Vector3 direction = Scatter(firingPoint.forward);

        Bullet bullet = GameObjectPoolController.Dequeue(poolableBulletKey).GetComponent<Bullet>();

        // recoil force
        parentRb.AddForceAtPosition(-direction * bullet.impactForce * knockbackModifier, origin, ForceMode.Impulse);

        bullet.Launch(origin, direction, gameObject);
        firingPoints[firingPointIndex].Fire();

        // increment firingpointindex
        firingPointIndex++;
        if (firingPointIndex >= firingPoints.Count)
            firingPointIndex = 0;
    }

    private Vector3 Scatter(Vector3 direction)
    {
        direction.x += Random.Range(-spread, spread);
        direction.y += Random.Range(-spread, spread);
        direction.z += Random.Range(-spread, spread);
        return direction.normalized;
    }
}
