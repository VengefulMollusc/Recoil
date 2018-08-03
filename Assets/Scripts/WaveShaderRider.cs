using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShaderRider : MonoBehaviour
{
    public WaveShaderPositionTracker wave;
    [Range(0, 1)] public float waveMovementStrength = 1f;
    [Range(0, 1)] public float tiltStrength = 1f;

    private Transform childTransform;

    void Start()
    {
        if (transform.childCount < 1)
        {
            Debug.LogError("No child object");
        }

        childTransform = transform.GetChild(0);
    }

    void Update()
    {
        Vector3 position = transform.position;
        WavePositionInfo info = wave.CalculateDepthAndNormalAtPoint(position);

        // match this object to wave vertical position
        transform.position = new Vector3(position.x, info.position.y, position.z);

        // apply wave horizontal movement to child transform
        if (waveMovementStrength > 0f)
            childTransform.localPosition = new Vector3(info.movement.x * waveMovementStrength, 0f, info.movement.z * waveMovementStrength);

        // apply wave normal tilt to child transform
        if (tiltStrength > 0f)
        {
            Vector3 tilt = new Vector3(info.normal.x * tiltStrength, info.normal.y, info.normal.z * tiltStrength).normalized;
            childTransform.rotation = Quaternion.FromToRotation(transform.up, tilt);
        }
    }
}
