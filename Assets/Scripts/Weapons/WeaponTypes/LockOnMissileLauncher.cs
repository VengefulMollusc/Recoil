using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA.Input;

public class LockOnMissileLauncher : Weapon
{
    public GameObject lockOnMissilePrefab;
    public int missileLaunchCount = 6;
    public float launchRate = 0.2f;
    public float lockOnAngle = 20f;
    public float lockOnRange = 200f;

    public List<Transform> launchPointTransforms;

    public LayerMask lockOnLayerMask;
    public LayerMask lockOnRayCastLayerMask;

    private List<LockOnTargetTracker> lockOnTargets;

    private const float lockOnCheckInterval = 0.1f;

    private Rigidbody parentRb;

    private string poolableMissileKey;

    private bool lockingOn;
    private bool firing;
    private int launchPointIndex;

    void Start()
    {
        poolableMissileKey = lockOnMissilePrefab.GetComponent<Poolable>().key;
        int poolableCount = missileLaunchCount;
        GameObjectPoolController.AddEntry(poolableMissileKey, lockOnMissilePrefab, poolableCount, poolableCount * ScenePlayerController.GetPlayerCount());

        parentRb = GetComponentInParent<Rigidbody>();

        if (launchPointTransforms.Count <= 0)
            Debug.LogError("No launchPointTransforms defined");
    }

    public override void FireWeapon(bool pressed)
    {
        lockingOn = pressed;
        if (lockingOn)
        {
            StartCoroutine(LockOnCoroutine());
            StartCoroutine(LockOnLevelUpdate());
        }
    }

    private IEnumerator LockOnLevelUpdate()
    {
        while (lockingOn)
        {
            if (!firing)
            {
                foreach (LockOnTargetTracker tracker in lockOnTargets)
                {
                    tracker.IncreaseLevel(Time.deltaTime);
                }
            }
            yield return null;
        }
    }

    private IEnumerator LockOnCoroutine()
    {
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
        if (lockOnTargets == null)
            lockOnTargets = new List<LockOnTargetTracker>();

        Vector3 forward = transform.forward;
        Vector3 origin = transform.position;
        float checkAngle = lockOnAngle * 0.5f;
        float radius = Mathf.Tan(checkAngle * Mathf.Deg2Rad) * lockOnRange;
        float rayLength = Mathf.Sqrt(lockOnRange * lockOnRange + radius * radius);

        foreach (LockOnTargetTracker tracker in lockOnTargets)
        {
            tracker.visible = false;
        }

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
                Transform targetTransform = target.transform;
                if (hitInfo.collider.transform == targetTransform)
                {
                    Debug.DrawLine(targetTransform.position, origin);

                    bool foundTracker = false;
                    foreach (LockOnTargetTracker tracker in lockOnTargets)
                    {
                        if (tracker.targetTransform == targetTransform)
                        {
                            tracker.visible = true;
                            foundTracker = true;
                            break;
                        }
                    }

                    if (!foundTracker && lockOnTargets.Count < missileLaunchCount)
                    {
                        LockOnTargetTracker newTracker = new LockOnTargetTracker(target);
                        lockOnTargets.Add(newTracker);
                    }
                }
            }
        }

        // Clear non-visible targets from tracking
        List<LockOnTargetTracker> trackers = new List<LockOnTargetTracker>(lockOnTargets);
        foreach (LockOnTargetTracker tracker in trackers)
        {
            if (!tracker.visible)
                lockOnTargets.Remove(tracker);
        }
    }

    private void Launch()
    {
        if (lockOnTargets.Count > 0)
        {
            firing = true;
            List<LockOnTargetTracker> targets = new List<LockOnTargetTracker>(lockOnTargets);
            StartCoroutine(LaunchCoroutine(targets));
        }
    }

    private IEnumerator LaunchCoroutine(List<LockOnTargetTracker> targets)
    {
        foreach (LockOnTargetTracker tracker in targets)
        {
            for (int i = 0; i < tracker.lockOnCount; i++)
            {
                LockOnMissile missile = GameObjectPoolController.Dequeue(poolableMissileKey).GetComponent<LockOnMissile>();
                Transform launchTransform = GetLaunchTransform();
                Vector3 launchPosition = launchTransform.position;
                Vector3 launchDirection = launchTransform.up;
                missile.Launch(launchPosition, launchTransform.forward, launchDirection, tracker.targetTransform, parentRb.velocity);

                // Add launch recoil force
                parentRb.AddForceAtPosition(-launchDirection * missile.launchForce, launchPosition, ForceMode.Impulse);
                yield return new WaitForSeconds(launchRate);
            }
        }

        lockOnTargets = null;
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

    private class LockOnTargetTracker
    {
        public readonly LockOnTarget target; 
        public readonly Transform targetTransform;
        public float lockOnLevel;
        public bool visible;
        public int lockOnCount;

        private float nextLockOnLevel;

        public LockOnTargetTracker(LockOnTarget target)
        {
            this.target = target;
            targetTransform = target.transform;
            visible = true;
            lockOnLevel = 0f;
            lockOnCount = 0;
            nextLockOnLevel = target.lockOnBaseTime;
        }

        public void IncreaseLevel(float delta)
        {
            if (lockOnCount >= target.maxLockOnCount)
                return;

            lockOnLevel += delta;

            if (lockOnLevel > nextLockOnLevel)
            {
                lockOnCount++;
                lockOnLevel = 0f;

                nextLockOnLevel *= 0.75f; // Defines lockon 'acceleration' for each successive lock
            }
        }
    }
}
