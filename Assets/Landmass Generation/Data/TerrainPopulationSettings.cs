using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainPopulationSettings : UpdatableData
{
    public List<PopulatableObject> populatableObjects;

    [Range(1, 5)]
    public int populationIndexStep;

    public List<GameObject> Populate(Transform terrainChunkTransform, HeightMap heightMap, MeshSettings meshSettings)
    {
        List<GameObject> population = new List<GameObject>();

        for (int x = 0; x < meshSettings.numVertsPerLine; x += populationIndexStep)
        {
            for (int y = 0; y < meshSettings.numVertsPerLine; y += populationIndexStep)
            {
                float pointHeight = heightMap.values[x, y];
                float relativeHeight = pointHeight / (heightMap.maxValue - heightMap.minValue);

                List<int> optionsAtThisheight = new List<int>();
                for (int i = 0; i < populatableObjects.Count; i++)
                {
                    if (relativeHeight > populatableObjects[i].minHeight && relativeHeight < populatableObjects[i].maxHeight)
                    {
                        optionsAtThisheight.Add(i);
                    }
                }

                if (optionsAtThisheight.Count <= 0)
                    break;

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
        //return new Vector2(x * (meshSettings.meshWorldSize / 10f), y * (meshSettings.meshWorldSize / 10f));
        return new Vector3(x - (meshSettings.numVertsPerLine / 2), pointHeight + terrainHeightOffset, (meshSettings.numVertsPerLine / 2) - y);
    }

    [System.Serializable]
    public class PopulatableObject
    {
        public GameObject objectPrefab;
        public float terrainHeightOffset;
        public float objectRadius;

        [Range(0, 1)] public float minHeight;
        [Range(0, 1)] public float maxHeight;
    }
}
