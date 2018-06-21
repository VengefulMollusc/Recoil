﻿using System.Collections;
using UnityEngine;

[CreateAssetMenu]
public class NoiseData : UpdatableData
{
    public Noise.NormaliseMode normaliseMode;

    public float noiseScale;

    public int octaves;
    [Range(0, 1)]
    public float persistance;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        if (lacunarity < 1f)
            lacunarity = 1f;

        if (octaves < 0)
            octaves = 0;

        base.OnValidate();
    }

#endif

}
