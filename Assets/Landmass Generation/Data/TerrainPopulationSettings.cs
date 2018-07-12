using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class TerrainPopulationSettings : UpdatableData
{
    public List<PopulatableObject> populatableObjects;

    public int populationIndexStep;

    public List<GameObject> Populate(Transform terrainChunkTransform, HeightMap heightMap, MeshSettings meshSettings)
    {
        List<GameObject> population = new List<GameObject>();

        for (int x = 0; x < meshSettings.numVertsPerLine; x += populationIndexStep)
        {
            for (int y = 0; y < meshSettings.numVertsPerLine; y += populationIndexStep)
            {
                float pointHeight = heightMap.values[x, y];



                Vector2 pos = GetPosition(x, y, meshSettings);

                PopulatableObject popObject = populatableObjects[0];

                GameObject newObject = GameObject.Instantiate(popObject.objectPrefab, terrainChunkTransform);
                newObject.transform.localPosition = new Vector3(pos.x, pointHeight + popObject.terrainHeightOffset, pos.y);

                population.Add(newObject);
            }
        }

        return population;
    }

    private Vector2 GetPosition(int x, int y, MeshSettings meshSettings)
    {
        return new Vector2(x * (meshSettings.meshWorldSize / 10f), y * (meshSettings.meshWorldSize / 10f));
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
