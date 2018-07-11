using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{

    public static HeightMap GenerateHeightMap(int width, HeightMapSettings settings, Vector2 sampleCenter)
    {
        float[,] values = Noise.GenerateNoiseMap(width, width, settings.noiseSettings, sampleCenter);

        AnimationCurve heightCurve_threadSafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                values[i, j] *= heightCurve_threadSafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }

    public static HeightMap GenerateHeightMapWithFalloff(int width, HeightMapSettings settings, Vector2 sampleCenter, float[,] falloffMap, int falloffStartX, int falloffStartY)
    {
        float[,] values = Noise.GenerateNoiseMap(width, width, settings.noiseSettings, sampleCenter);

        AnimationCurve heightCurve_threadSafe = new AnimationCurve(settings.heightCurve.keys);

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                // apply falloff map
                float falloffValue = CalculateFalloffValue(falloffStartX + i, falloffStartY + j, falloffMap);
                values[i, j] -= falloffValue;

                if (values[i, j] < 0f)
                    values[i, j] = 0f;

                values[i, j] *= heightCurve_threadSafe.Evaluate(values[i, j]) * settings.heightMultiplier;

                if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }
            }
        }

        return new HeightMap(values, minValue, maxValue);
    }

    private static float CalculateFalloffValue(int x, int y, float[,] falloffMap)
    {
        int falloffMapSize = falloffMap.GetLength(0);
        bool xInBounds = x > 0 && x < falloffMapSize;
        bool yInBounds = y > 0 && y < falloffMapSize;

        if (xInBounds && yInBounds)
        {
            // both are in bounds. return value as normal
            return falloffMap[x, y];
        }

        // coords are outside falloffMap
        int halfFalloffSize = falloffMapSize / 2;
        int lowerZeroBound = -halfFalloffSize;
        int higherZeroBound = falloffMapSize + halfFalloffSize;
        if (x < lowerZeroBound || x >= higherZeroBound || y < lowerZeroBound || y >= higherZeroBound)
        {
            // coords outside inverted map effect area
            return 0f;
        }

        // invert falloff effects here for outer border
        if (!xInBounds && !yInBounds)
        {
            // neither index is in bounds
            x = GetAbsoluteIndex(x, falloffMapSize);
            y = GetAbsoluteIndex(y, falloffMapSize);

            if (x > y)
                y = halfFalloffSize;
            else
                x = halfFalloffSize;
        }
        else
        {
            // one index is within bounds
            x = yInBounds ? GetAbsoluteIndex(x, falloffMapSize) : halfFalloffSize;
            y = xInBounds ? GetAbsoluteIndex(y, falloffMapSize) : halfFalloffSize;
        }

        return falloffMap[x, y];
    }

    /*
     * Calculates the absolute index of a falloffMap coordinate (the relative index INSIDE the bounds of the map)
     */
    private static int GetAbsoluteIndex(int index, int falloffMapSize)
    {
        if (index < 0)
            index = -index;
        else if (index >= falloffMapSize)
            index -= falloffMapSize;

        return index;
    }
}

public struct HeightMap
{
    public readonly float[,] values;
    public readonly float minValue;
    public readonly float maxValue;

    public HeightMap(float[,] values, float minValue, float maxValue)
    {
        this.values = values;
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
