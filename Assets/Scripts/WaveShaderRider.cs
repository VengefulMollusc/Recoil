using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShaderRider : MonoBehaviour
{

    public WaveShaderPositionTracker wave;
    public bool trackVerticalOnly;

    void Update()
    {
        WavePositionInfo info = wave.CalculateDepthAndNormalAtPoint(transform.position);
        if (trackVerticalOnly)
            transform.position = new Vector3(transform.position.x, info.position.y, transform.position.z);
        else
            transform.position = info.position;
        transform.rotation = Quaternion.LookRotation(info.normal);
    }
}
