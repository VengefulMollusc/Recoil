using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TerrainPopulator
{
    public static List<GameObject> Populate(TerrainPopulationSettings populationSettings, Transform terrainChunkTransform, HeightMap heightMap, MeshSettings meshSettings, HeightMapSettings heightMapSettings, Vector2 chunkCenter)
    {
        List<GameObject> population = new List<GameObject>();

        List<PopulatableObject> populatableObjects_threadSafe = new List<PopulatableObject>(populationSettings.populatableObjects);
        AnimationCurve heightCurve_threadSafe = new AnimationCurve(heightMapSettings.heightCurve.keys);

        for (int x = 1; x < meshSettings.numVertsPerLine - 1; x += populationSettings.populationIndexStep)
        {
            for (int y = 1; y < meshSettings.numVertsPerLine - 1; y += populationSettings.populationIndexStep)
            {
                float pointHeight = heightMap.values[x, y];
                float noiseVal = Mathf.PerlinNoise(x + chunkCenter.x + pointHeight, y + chunkCenter.y + pointHeight);

                int popObjectIndex = -1;
                for (int i = 0; i < populatableObjects_threadSafe.Count; i++)
                {
                    float min = populatableObjects_threadSafe[i]
                        .GetMinHeight(heightCurve_threadSafe, heightMapSettings.heightMultiplier);
                    float max = populatableObjects_threadSafe[i]
                        .GetMaxHeight(heightCurve_threadSafe, heightMapSettings.heightMultiplier);
                    if (noiseVal < populatableObjects_threadSafe[i].noiseThreshold && pointHeight >= min && pointHeight <= max)
                    {
                        if (popObjectIndex < 0 || 
                            populatableObjects_threadSafe[i].noiseThreshold < populatableObjects_threadSafe[popObjectIndex].noiseThreshold)
                        {
                            popObjectIndex = i;
                        }
                    }
                }

                if (popObjectIndex < 0)
                    continue;

                PopulatableObject popObject = populatableObjects_threadSafe[popObjectIndex];

                GameObject newObject = GameObject.Instantiate(popObject.objectPrefab, terrainChunkTransform);

                Vector3 pos = GetPosition(x, y, pointHeight, popObject.terrainHeightOffset, meshSettings);
                newObject.transform.localPosition = pos;
                newObject.transform.localScale = Vector3.one * 10f;

                population.Add(newObject);
            }
        }

        return population;
    }

    private static Vector3 GetPosition(int x, int y, float pointHeight, float terrainHeightOffset, MeshSettings meshSettings)
    {
        float xPos = (x - (meshSettings.numVertsPerLine / 2)) * meshSettings.meshScale;
        float zPos = ((meshSettings.numVertsPerLine / 2) - y) * meshSettings.meshScale;
        float height = pointHeight + terrainHeightOffset;

        return new Vector3(xPos, height, zPos);
    }

}

[System.Serializable]
public class PopulatableObject
{
    public GameObject objectPrefab;
    public float terrainHeightOffset;
    public float placementRadius;

    [Range(0, 1)] public float noiseThreshold;

    [Range(0, 1)] [SerializeField] private float minHeight;
    [Range(0, 1)] [SerializeField] private float maxHeight;

    public float GetMinHeight(AnimationCurve heightCurve, float heightMultiplier)
    {
        return heightCurve.Evaluate(minHeight) * heightMultiplier;
    }

    public float GetMaxHeight(AnimationCurve heightCurve, float heightMultiplier)
    {
        return heightCurve.Evaluate(maxHeight) * heightMultiplier;
    }
}

