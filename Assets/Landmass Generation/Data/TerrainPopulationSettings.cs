using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainPopulationSettings : UpdatableData
{
    [Range(1, 5)]
    public int populationIndexStep;

    public List<PopulatableObject> populatableObjects;

    public List<GameObject> Populate(Transform terrainChunkTransform, HeightMap heightMap, MeshSettings meshSettings, HeightMapSettings heightMapSettings)
    {
        List<GameObject> population = new List<GameObject>();

        AnimationCurve heightCurve_threadSafe = new AnimationCurve(heightMapSettings.heightCurve.keys);

        for (int x = 1; x < meshSettings.numVertsPerLine-1; x += populationIndexStep)
        {
            for (int y = 1; y < meshSettings.numVertsPerLine-1; y += populationIndexStep)
            {
                float pointHeight = heightMap.values[x, y];

                List<int> optionsAtThisheight = new List<int>();
                for (int i = 0; i < populatableObjects.Count; i++)
                {
                    float min = populatableObjects[i]
                        .GetMinHeight(heightCurve_threadSafe, heightMapSettings.heightMultiplier);
                    float max = populatableObjects[i]
                        .GetMaxHeight(heightCurve_threadSafe, heightMapSettings.heightMultiplier);
                    if (pointHeight >= min && pointHeight <= max)
                    {
                        optionsAtThisheight.Add(i);
                    }
                }

                if (optionsAtThisheight.Count <= 0)
                    continue;

                //float noiseVal = Mathf.PerlinNoise(x + pointHeight, y + pointHeight);

                //if (noiseVal < 0.5f)
                //    break;

                int popObjectIndex = Random.Range(0, optionsAtThisheight.Count);
                PopulatableObject popObject = populatableObjects[popObjectIndex];

                GameObject newObject = GameObject.Instantiate(popObject.objectPrefab, terrainChunkTransform);

                Vector3 pos = GetPosition(x, y, pointHeight, popObject.terrainHeightOffset, meshSettings);
                newObject.transform.localPosition = pos;
                newObject.transform.localScale = Vector3.one * 10f;

                population.Add(newObject);
            }
        }

        return population;
    }

    private Vector3 GetPosition(int x, int y, float pointHeight, float terrainHeightOffset, MeshSettings meshSettings)
    {
        float xPos = (x - (meshSettings.numVertsPerLine / 2)) * meshSettings.meshScale;
        float zPos = ((meshSettings.numVertsPerLine / 2) - y) * meshSettings.meshScale;
        float height = pointHeight + terrainHeightOffset;

        return new Vector3(xPos, height, zPos);
    }

    [System.Serializable]
    public class PopulatableObject
    {
        public GameObject objectPrefab;
        public float terrainHeightOffset;
        public float placementRadius;

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
}
