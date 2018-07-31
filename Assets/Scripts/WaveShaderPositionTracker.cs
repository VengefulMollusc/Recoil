﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveShaderPositionTracker : MonoBehaviour
{
    public Material waveMaterial;
    public Transform targetObjectTransform;

    void Update()
    {
        Vector3 position = targetObjectTransform.position;
        waveMaterial.SetVector("_PlayerPosition", new Vector4(position.x, position.y, position.z, 0));
    }
}
