using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShaderRider : MonoBehaviour
{

    public WaveShaderPositionTracker wave;

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
        transform.position = new Vector3(position.x, info.position.y, position.z);

        //Vector2 horMovement = new Vector2(info.position.x - position.x, info.position.z - position.z) * 0.1f;
        //transform.position = new Vector3(position.x + horMovement.x, info.position.y, position.z + horMovement.y);

        childTransform.localPosition = new Vector3(info.movement.x, 0f, info.movement.z);
        childTransform.rotation = Quaternion.LookRotation(transform.forward, info.normal);
    }
}
