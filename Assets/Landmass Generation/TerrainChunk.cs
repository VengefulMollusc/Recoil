using System.Collections.Generic;
using UnityEngine;

public class TerrainChunk
{
    private const float colliderGenerationDistanceThreshold = 10f; // 10f
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    private GameObject meshObject;
    private Transform meshTransform;
    private Vector2 sampleCentre;
    private Bounds bounds;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private Rigidbody rigidBody;

    private LODInfo[] detailLevels;
    private LODMesh[] lodMeshes;
    private int colliderLODIndex;

    private HeightMap heightMap;
    private bool heightMapReceived;
    private int previousLODIndex = -1;
    private bool hasSetCollider;
    private float maxViewDist;

    private HeightMapSettings heightMapSettings;
    private MeshSettings meshSettings;
    private List<Transform> viewers;

    private TerrainPopulationSettings populationSettings;
    private List<GameObject> populationObjects;

    private bool isInBoundsChunk;


    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, List<Transform> viewers, Material material, TerrainPopulationSettings populationSettings)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewers = viewers;
        this.populationSettings = populationSettings;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshObject.layer = 11;
        meshTransform = meshObject.transform;
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        rigidBody = meshObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true;
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < lodMeshes.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;

    }

    public void Load(bool isInBounds, bool isFlatChunk)
    {
        isInBoundsChunk = isInBounds;
        if (isFlatChunk)
        {
            // Create flat heightMap
            float[,] values = new float[meshSettings.numVertsPerLine, meshSettings.numVertsPerLine];
            for (int x = 0; x < meshSettings.numVertsPerLine; x++)
            {
                for (int y = 0; y < meshSettings.numVertsPerLine; y++)
                {
                    values[x, y] = 0f;
                }
            }
            HeightMap heightMap = new HeightMap(values, 0f, 0f);
            OnHeightMapReceived(heightMap);
        }
        else
        {
            ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, heightMapSettings, sampleCentre), OnHeightMapReceived);
        }
    }

    public void Load(bool isInBounds, float[,] falloffMap, int falloffStartX, int falloffStartY)
    {
        isInBoundsChunk = isInBounds;
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMapWithFalloff(meshSettings.numVertsPerLine, heightMapSettings, sampleCentre, falloffMap, falloffStartX, falloffStartY), OnHeightMapReceived);
    }

    void OnHeightMapReceived(object heightMapObject)
    {
        heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;
        
        UpdateTerrainChunk();

        if (isInBoundsChunk)
        {
            // Populate chunk once heightmap received
            //ThreadedDataRequester.RequestData(() => TerrainPopulator.Populate(populationSettings, meshTransform, heightMap, meshSettings, heightMapSettings, sampleCentre), OnPopulationReceived);
            Populate();
        }
    }

    void Populate()
    {
        populationObjects = TerrainPopulator.Populate(populationSettings, meshTransform, heightMap, meshSettings, heightMapSettings, sampleCentre);
    }

    //void OnPopulationReceived(object populationListObject)
    //{
    //    populationObjects = (List<GameObject>)populationListObject;
    //}

    private float closestViewerDistance
    {
        get
        {
            float closest = float.MaxValue;
            foreach (Transform viewer in viewers)
                closest = Mathf.Min(closest, bounds.SqrDistance(new Vector2(viewer.position.x, viewer.position.z)));
            return closest;
        }
    }

    public void UseWaterPlane(Mesh waterMesh)
    {
        GameObject waterPlaneObject = new GameObject("WaterPlane")
        {
            layer = 11
        };
        waterPlaneObject.transform.SetParent(meshObject.transform);

        MeshFilter waterPlaneMeshFilter = waterPlaneObject.AddComponent<MeshFilter>();
        waterPlaneMeshFilter.sharedMesh = waterMesh;
        MeshCollider waterPlaneMeshCollider = waterPlaneObject.AddComponent<MeshCollider>();
        waterPlaneMeshCollider.sharedMesh = waterMesh;
        MeshRenderer waterPlaneMeshRenderer = waterPlaneObject.AddComponent<MeshRenderer>();
        waterPlaneMeshRenderer.sharedMaterial = heightMapSettings.waterMaterial;

        // set height and scale for water plane
        float waterHeight = heightMapSettings.heightCurve.Evaluate(heightMapSettings.waterHeight) *
                            heightMapSettings.heightMultiplier;
        waterPlaneObject.transform.position = new Vector3(meshObject.transform.position.x, waterHeight, meshObject.transform.position.z);
    }

    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewerDistFromNearestEdge = Mathf.Sqrt(closestViewerDistance);

            bool wasVisible = IsVisible();
            bool visible = viewerDistFromNearestEdge <= maxViewDist;

            if (visible)
            {
                int lodIndex = 0;

                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                    {
                        lodMesh.RequestMesh(heightMap, meshSettings);
                    }
                }
            }

            if (wasVisible != visible)
            {
                SetVisible(visible);
                if (onVisibilityChanged != null)
                {
                    onVisibilityChanged(this, visible);
                }
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (!hasSetCollider)
        {
            float sqrDistFromViewerToEdge = closestViewerDistance;

            if (isInBoundsChunk || sqrDistFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDistanceThreshold)
            {
                if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
                {
                    lodMeshes[colliderLODIndex].RequestMesh(heightMap, meshSettings);
                }
            }

            if (isInBoundsChunk || sqrDistFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if (lodMeshes[colliderLODIndex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }

    public bool IsLoaded()
    {
        return hasSetCollider;
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }

}

class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    private int lod;
    public event System.Action updateCallback;

    public LODMesh(int lod)
    {
        this.lod = lod;
    }

    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }
}
