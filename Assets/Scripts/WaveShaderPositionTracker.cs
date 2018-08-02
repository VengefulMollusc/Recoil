using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShaderPositionTracker : MonoBehaviour
{
    public Material waveMaterial;
    public Transform targetObjectTransform;
    public float heightOffset;

    private Vector3 trackedPosition;

    void Update()
    {
        Vector3 newPosition = targetObjectTransform.position;

        // accelerate movement of y position
        float newY = trackedPosition.y + (newPosition.y - trackedPosition.y) * Time.deltaTime * 10f;
        trackedPosition = new Vector3(trackedPosition.x, newY, trackedPosition.z);

        // smooth movement
        float distanceMoved = Vector3.SqrMagnitude(newPosition - trackedPosition);
        trackedPosition = Vector3.MoveTowards(trackedPosition, newPosition, distanceMoved * Time.deltaTime);

        waveMaterial.SetVector("_PlayerPosition", new Vector4(trackedPosition.x, trackedPosition.y + heightOffset, trackedPosition.z, 0));
    }
}
