using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FalloffGenerator {

    public enum FalloffMode
    {
        Square,
        Circular
    };

    public static float[,] GenerateFalloffMap(int size, FalloffMode mode)
    {
        float[,] map = new float[size,size];

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float x = i / (float) size * 2 - 1;
                float y = j / (float) size * 2 - 1;

                float value = 0f;

                if (mode == FalloffMode.Square)
                    value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                else if (mode == FalloffMode.Circular)
                    value = Mathf.Min(Mathf.Sqrt(x * x + y * y), 1f);

                map[i, j] = Evaluate(value);
            }
        }

        return map;
    }

    static float Evaluate(float value)
    {
        float a = 3f;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
