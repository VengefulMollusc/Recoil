using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(FiringPoint))]
public class AutoTargeter : MonoBehaviour
{
    public LayerMask layerMask;
    public LayerMask raycastMask;
    public float checkAngle;
    public float range;
    public List<AutoTargeter> otherTargeters;

    private const float checkFreq = 0.2f;
    private const float aimSpeed = 90f;

    private Weapon weapon;
    private LockOnTarget target;

    private Vector3 parentForward;
    private Vector3 position;
    private float radiusAngle;
    private float targetDistance;

    void OnEnable()
    {
        weapon = GetComponentInParent<Weapon>();

        radiusAngle = checkAngle * 0.5f;

        // random float here to offset checks so all autotargeters arent called on same frame
        InvokeRepeating("TargetCheck", Random.Range(0f, checkFreq), checkFreq);
    }

    void OnDisable()
    {
        CancelInvoke("TargetCheck");

        target = null;
        Aim();
    }

    void Update()
    {
        Aim();
    }

    void TargetCheck()
    {
        if (!weapon.IsActive())
            return;

        parentForward = weapon.transform.forward;
        position = transform.position;

        if (target != null)
        {
            targetDistance = ViableTargetDistance(target);
            if (targetDistance < 0f)
                target = null;
        }

        FindTarget();
    }

    void FindTarget()
    {
        float radius = Mathf.Tan(radiusAngle * Mathf.Deg2Rad) * range;

        Collider[] cols = Physics.OverlapCapsule(position + (parentForward * radius), position + (parentForward * range), radius, layerMask,
            QueryTriggerInteraction.Ignore);

        List<LockOnTarget> alreadyTargeted = new List<LockOnTarget>();

        foreach (Collider col in cols)
        {
            LockOnTarget t = col.GetComponent<LockOnTarget>();
            if (IsAlreadyTargeted(t))
            {
                alreadyTargeted.Add(t);
                continue;
            }

            if (t == null)
                continue;

            float distance = ViableTargetDistance(t);
            if (distance < 0f)
                continue;

            if (target == null || distance < targetDistance)
            {
                target = t;
                targetDistance = distance;
            }
        }

        if (target == null && alreadyTargeted.Count > 0)
        {
            target = alreadyTargeted[Random.Range(0, alreadyTargeted.Count)];
        }
    }

    void Aim()
    {
        Vector3 currentAimDirection = transform.forward;
        Vector3 targetAimDirection = (target == null) ? parentForward : target.transform.position - position;

        targetAimDirection =
            Vector3.RotateTowards(currentAimDirection, targetAimDirection, aimSpeed * Mathf.Deg2Rad * Time.deltaTime, 0f);

        transform.rotation = Quaternion.LookRotation(targetAimDirection);
    }

    float ViableTargetDistance(LockOnTarget t)
    {
        Vector3 tPos = t.transform.position;
        float angle = Vector3.Angle(parentForward, tPos - position);
        if (angle > radiusAngle)
            return -1f;

        RaycastHit hitInfo;
        Vector3 toTarget = tPos - position;
        if (Physics.Raycast(position, toTarget, out hitInfo, range, raycastMask))
        {
            if (hitInfo.collider.transform == t.transform)
                return hitInfo.distance;
        }

        return -1f;
    }

    bool IsAlreadyTargeted(LockOnTarget t)
    {
        foreach (AutoTargeter targeter in otherTargeters)
        {
            if (targeter.IsTarget(t))
                return true;
        }
        return false;
    }

    public bool IsTarget(LockOnTarget other)
    {
        return other == target;
    }
}
