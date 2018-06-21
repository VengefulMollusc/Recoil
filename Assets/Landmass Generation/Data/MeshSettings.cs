﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class MeshSettings : UpdatableData
{
    public const int numSupportedLODs = 5;
    public const int numSupportedChunkSizes = 9;
    public const int numSupportedFlatShadedChunkSizes = 3;
    public static readonly int[] supportedChunkSizes = { 48, 72, 96, 120, 144, 168, 192, 216, 240 };

    public float meshScale = 5f;
    public bool useFlatShading;

    [Range(0, numSupportedChunkSizes - 1)]
    public int chunkSizeIndex;
    [Range(0, numSupportedFlatShadedChunkSizes - 1)]
    public int flatShadedChunkSizeIndex;


    // num of vertices per line of a mesh rendered at LOD = 0 (including border vertices used for calculating normals)
    public int numVertsPerLine
    {
        get
        {
            return supportedChunkSizes[useFlatShading ? flatShadedChunkSizeIndex : chunkSizeIndex] + 1;
        }
    }

    public float meshWorldSize
    {
        get { return (numVertsPerLine - 3) * meshScale; }
    }
}
