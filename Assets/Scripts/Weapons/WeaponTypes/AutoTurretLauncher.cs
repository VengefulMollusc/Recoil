using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoTurretLauncher : UtilityWeapon
{
    public GameObject turretPrefab;
    public int maxTurrets;
    public float fireRate;
    public FiringPoint firingPoint;
    public bool useRecoil;

    private string poolableTurretKey;

    private bool firing;
    private bool firingSequenceActive;

    private Rigidbody parentRb;
    private List<AutoTurret> turrets;

    void Start()
    {
        poolableTurretKey = turretPrefab.GetComponent<Poolable>().key;
        int poolableCount = maxTurrets + 2;
        GameObjectPoolController.AddEntry(poolableTurretKey, turretPrefab, poolableCount, poolableCount * ScenePlayerController.GetPlayerCount());

        if (firingPoint == null)
            Debug.LogError("No firingPoints defined");

        parentRb = GetComponentInParent<Rigidbody>();
        turrets = new List<AutoTurret>();
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
            LaunchTurret();
            yield return new WaitForSeconds(fireRate);
        }

        firingSequenceActive = false;
    }

    void LaunchTurret()
    {
        Vector3 origin = firingPoint.transform.position;
        Vector3 direction = firingPoint.transform.forward;

        AutoTurret turret = GameObjectPoolController.Dequeue(poolableTurretKey).GetComponent<AutoTurret>();

        if (turrets.Count >= maxTurrets)
        {
            turrets[0].TriggerDespawn();
            turrets.RemoveAt(0);
        }
        turrets.Add(turret);

        // recoil force
        if (useRecoil)
            parentRb.AddForceAtPosition(-direction * turret.launchForce * knockbackModifier, origin, ForceMode.Impulse);

        turret.Launch(origin, direction, owner);
        firingPoint.Fire();
    }
}
