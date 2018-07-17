using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : Weapon
{
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;
    public float spread = 0.02f;
    public List<Vector3> firingPoints;

    [Header("Editor")] public bool editFiringPoints;

    private bool firing;
    private bool firingSequenceActive;

    private string poolableBulletKey;
    private int firingPointIndex;

    void Start()
    {
        poolableBulletKey = bulletPrefab.GetComponent<Poolable>().key;
        int poolableCount = (int)(bulletPrefab.GetComponent<Bullet>().lifeSpan / fireRate);
        GameObjectPoolController.AddEntry(poolableBulletKey, bulletPrefab, poolableCount, poolableCount * 4);
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
        Vector3 origin = GetFiringPoint();
        Vector3 direction = GetDirection();

        Bullet bullet = GameObjectPoolController.Dequeue(poolableBulletKey).GetComponent<Bullet>();
        bullet.Launch(origin, direction, gameObject);
    }

    private Vector3 GetFiringPoint()
    {
        Vector3 point = transform.position + firingPoints[firingPointIndex];
        firingPointIndex++;
        if (firingPointIndex >= firingPoints.Count)
            firingPointIndex = 0;
        return point;
    }

    private Vector3 GetDirection()
    {
        Vector3 direction = transform.forward;
        direction.x += Random.Range(-spread, spread);
        direction.y += Random.Range(-spread, spread);
        direction.z += Random.Range(-spread, spread);
        return direction;
    }
}
