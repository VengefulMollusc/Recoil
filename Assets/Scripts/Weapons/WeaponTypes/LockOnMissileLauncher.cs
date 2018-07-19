using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockOnMissileLauncher : Weapon
{
    public GameObject lockOnMissilePrefab;
    public int missileLaunchCount = 6;
    public float launchRate = 0.2f;
    public float lockOnAngle = 20f;
    public float lockOnRange = 200f;
    public float lockOnTime = 1f;

    public List<Transform> launchPointTransforms;

    public LayerMask lockOnLayerMask;
    public LayerMask lockOnRayCastLayerMask;

    private const float lockOnCheckInterval = 0.1f;

    private List<LockOnTarget> lockOnTargets;
    private List<float> lockOnLevels;

    private Rigidbody parentRb;

    private string poolableMissileKey;

    private bool lockingOn;
    private bool firing;
    private int launchPointIndex;

    void Start()
    {
        poolableMissileKey = lockOnMissilePrefab.GetComponent<Poolable>().key;
        int poolableCount = missileLaunchCount;
        GameObjectPoolController.AddEntry(poolableMissileKey, lockOnMissilePrefab, poolableCount, poolableCount * 4);

        parentRb = GetComponentInParent<Rigidbody>();

        if (launchPointTransforms.Count <= 0)
            Debug.LogError("No launchPointTransforms defined");
    }

    public override void FireWeapon(bool pressed)
    {
        lockingOn = pressed;
        if (pressed)
        {
            StartCoroutine(LockOnCoroutine());
        }
    }

    private IEnumerator LockOnCoroutine()
    {
        lockOnTargets = new List<LockOnTarget>();
        lockOnLevels = new List<float>();

        while (lockingOn)
        {
            if (!firing)
                LockOn();

            yield return new WaitForSeconds(lockOnCheckInterval);
        }

        Launch();
    }

    private void LockOn()
    {
        Vector3 forward = transform.forward;
        Vector3 origin = transform.position;
        float checkAngle = lockOnAngle * 0.5f;
        float radius = Mathf.Tan(checkAngle * Mathf.Deg2Rad) * lockOnRange;
        float rayLength = Mathf.Sqrt(lockOnRange * lockOnRange + radius * radius);

        List<LockOnTarget> visibleTargets = new List<LockOnTarget>();

        Collider[] cols = Physics.OverlapCapsule(origin + (forward * radius), origin + (forward * lockOnRange), radius, lockOnLayerMask,
            QueryTriggerInteraction.Ignore);

        foreach (Collider col in cols)
        {
            LockOnTarget target = col.GetComponent<LockOnTarget>();
            if (target == null)
                continue;

            RaycastHit hitInfo;
            Vector3 targetPos = target.transform.position;
            Vector3 toTarget = targetPos - origin;
            float targetAngle = Vector3.Angle(forward, toTarget);

            //TODO: Layer mask here just includes scenery and lockontargets
            if (targetAngle <= checkAngle && Physics.Raycast(origin, toTarget, out hitInfo, rayLength, lockOnRayCastLayerMask))
            {
                if (hitInfo.collider.transform == target.transform)
                {
                    visibleTargets.Add(target);

                    if (!lockOnTargets.Contains(target))
                    {
                        if (lockOnTargets.Count < missileLaunchCount)
                        {
                            lockOnTargets.Add(target);
                            lockOnLevels.Add(0);
                        }
                    }
                    else
                    {
                        int index = lockOnTargets.IndexOf(target);
                        //if (lockOnLevels[index] < 1f)
                            lockOnLevels[index] += lockOnCheckInterval;

                        //if (lockOnLevels[index] > 1f)
                        //    lockOnLevels[index] = 1f;
                    }
                }
            }
        }

        // Clear non-visible targets from tracking
        for (int i = lockOnTargets.Count - 1; i >= 0; i--)
        {

            Debug.DrawLine(transform.position, lockOnTargets[i].transform.position);

            if (!visibleTargets.Contains(lockOnTargets[i]))
            {
                lockOnTargets.RemoveAt(i);
                lockOnLevels.RemoveAt(i);
            }
        }
    }

    private void Launch()
    {
        if (lockOnTargets.Count > 0)
        {
            firing = true;
            List<LockOnTarget> targets = new List<LockOnTarget>(lockOnTargets);
            List<float> levels = new List<float>(lockOnLevels);
            StartCoroutine(LaunchCoroutine(targets, levels));
        }
    }

    private IEnumerator LaunchCoroutine(List<LockOnTarget> targets, List<float> levels)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            if (levels[i] < 1f)
                continue;

            LockOnMissile missile = GameObjectPoolController.Dequeue(poolableMissileKey).GetComponent<LockOnMissile>();
            Transform launchTransform = GetLaunchTransform();
            missile.Launch(launchTransform.position, launchTransform.forward, launchTransform.up, targets[i].transform, parentRb.velocity);
            yield return new WaitForSeconds(launchRate);
        }

        firing = false;
    }

    private Transform GetLaunchTransform()
    {
        Transform launchTransform = launchPointTransforms[launchPointIndex];
        launchPointIndex++;
        if (launchPointIndex >= launchPointTransforms.Count)
            launchPointIndex = 0;
        return launchTransform;
    }
}
