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
        string glyphs = "abcdefghijklmnopqrstuvwxyz0123456789";
        int seedLength = Random.Range(3, 6);
        string seedString = "";
        for (int i = 0; i < seedLength; i++)
        {
            seedString += glyphs[Random.Range(0, glyphs.Length)];
        }
        noiseSettings.seedString = seedString;

        noiseSettings.ValidateValues();
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        noiseSettings.ValidateValues();
        base.OnValidate();
    }

#endif

}
