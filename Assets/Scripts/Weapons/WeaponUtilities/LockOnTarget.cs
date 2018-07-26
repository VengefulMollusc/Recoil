using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class LockOnTarget : MonoBehaviour
{
    public int maxLockOnCount = 1;
    public float lockOnBaseTime = 1f;

    private GameObject owner;

    public GameObject GetOwner()
    {
        return owner;
    }

    public void SetOwner(GameObject newOwner)
    {
        owner = newOwner;
    }
}
