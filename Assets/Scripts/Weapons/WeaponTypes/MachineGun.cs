using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGun : Weapon
{
    public GameObject bulletPrefab;
    public float fireRate = 0.2f;

    private bool firing;
    private bool firingSequenceActive;

    private string poolableBulletKey;

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
        Vector3 origin = transform.position + (transform.up * 0.5f);
        Vector3 direction = transform.forward;

        Bullet bullet = GameObjectPoolController.Dequeue(poolableBulletKey).GetComponent<Bullet>();
        bullet.Launch(origin, direction, gameObject);
    }
}
