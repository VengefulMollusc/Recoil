using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu]
public class HeightMapSettings : UpdatableData
{
    public NoiseSettings noiseSettings;
    public bool useFalloff;

    public FalloffGenerator.FalloffMode falloffMode;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minHeight
    {
        get { return heightMultiplier * heightCurve.Evaluate(0); }
    }

    public float maxHeight
    {
        get { return heightMultiplier * heightCurve.Evaluate(1); }
    }

    public void RandomiseNoise()
    {
        noiseSettings.seed = Random.Range(-9999, 9999);
        noiseSettings.offset = new Vector2(Random.Range(-9999f, 9999f), Random.Range(-9999f, 9999f));
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }

#endif

}
