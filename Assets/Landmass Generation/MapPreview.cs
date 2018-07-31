using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapPreview : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    public enum DrawMode
    {
        NoiseMap,
        DrawMesh,
        FalloffMap
    };

    public DrawMode drawMode;

    public TerrainDataPackage terrainDataPackage;
    private HeightMapSettings heightMapSettings;
    private MeshSettings meshSettings;
    private TextureData textureData;

    public Material terrainMaterial;

    public Transform previewParent;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;

    public bool previewWholeFixedSizeMesh;

    public bool autoUpdate;

    private List<GameObject> previewChunks;

    private Mesh waterMesh;

    private void UpdateTerrainDataVariables()
    {
        heightMapSettings = terrainDataPackage.heightMapSettings;
        meshSettings = terrainDataPackage.meshSettings;
        textureData = terrainDataPackage.textureData;
    }

    public void DrawMapInEditor()
    {
        UpdateTerrainDataVariables();
        UpdateDataCallbacks();

        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        int previewSize = meshSettings.numVertsPerLine;
        int gridSize = 1;

        if (meshSettings.generateFixedSizeTerrain)
        {
            gridSize = meshSettings.fixedTerrainSize * 2 + 1;
            previewSize = (meshSettings.numVertsPerLine - 3) * gridSize + 3;
        }

        if (heightMapSettings.useWaterPlane)
            waterMesh = MeshGenerator.GenerateWaterMesh(meshSettings, editorPreviewLOD).CreateMesh();

        if (drawMode != DrawMode.DrawMesh || !(previewWholeFixedSizeMesh && meshSettings.generateFixedSizeTerrain))
        {
            ClearPreviewChunks();
        }

        if (drawMode == DrawMode.NoiseMap)
        {
            float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(previewSize, heightMapSettings.falloffMode);
            HeightMap heightMap = (heightMapSettings.useFalloff) ? HeightMapGenerator.GenerateHeightMapWithFalloff(previewSize, heightMapSettings, Vector2.zero, falloffMap, 0, 0) :
                HeightMapGenerator.GenerateHeightMap(previewSize, heightMapSettings, Vector2.zero);
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap), gridSize);
        }
        else if (drawMode == DrawMode.DrawMesh)
        {
            if (previewWholeFixedSizeMesh && meshSettings.generateFixedSizeTerrain)
            {
                PreviewFixedSizeMesh(previewSize);
            }
            else
            {
                HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);
                DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
            }
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(previewSize, heightMapSettings.falloffMode), 0, 1)), gridSize);
        }
    }

    void ClearPreviewChunks()
    {
        if (previewChunks == null)
        {
            previewChunks = new List<GameObject>();
            return;
        }
        foreach (GameObject chunk in previewChunks)
        {
            if (chunk != null)
                DestroyImmediate(chunk);
        }
        previewChunks.Clear();
    }

    private void PreviewFixedSizeMesh(int previewSize)
    {
        int fixedTerrainSize = meshSettings.fixedTerrainSize;

        float[,] falloffMap = (heightMapSettings.useFalloff) ? FalloffGenerator.GenerateFalloffMap(previewSize, heightMapSettings.falloffMode) : new float[0, 0];

        int previewChunkIndex = 0;

        if (previewChunks == null)
        {
            previewChunks = new List<GameObject>();
            foreach (GameObject chunk in GameObject.FindGameObjectsWithTag("PreviewChunk"))
            {
                previewChunks.Add(chunk);
            }
        }
        else if (previewChunks.Count > 0 && previewChunks[0] == null)
        {
            ClearPreviewChunks();
        }

        for (int x = -fixedTerrainSize; x <= fixedTerrainSize; x++)
        {
            for (int y = -fixedTerrainSize; y <= fixedTerrainSize; y++)
            {
                Vector2 position = new Vector2(x * meshSettings.meshWorldSize, y * meshSettings.meshWorldSize);
                Vector2 sampleCenter = position / meshSettings.meshScale;
                HeightMap heightMap;

                if (heightMapSettings.useFalloff)
                {
                    int falloffStartX = (meshSettings.numVertsPerLine - 3) * (x + fixedTerrainSize);
                    int falloffStartY = previewSize - 3 - (meshSettings.numVertsPerLine - 3) * (y + fixedTerrainSize + 1);

                    heightMap = HeightMapGenerator.GenerateHeightMapWithFalloff(meshSettings.numVertsPerLine, heightMapSettings,
                        sampleCenter, falloffMap, falloffStartX, falloffStartY);
                }
                else
                {
                    heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, heightMapSettings,
                        sampleCenter);
                }

                GameObject previewMeshObject;
                MeshFilter previewMeshFilter;
                if (previewChunkIndex < previewChunks.Count)
                {
                    previewMeshObject = previewChunks[previewChunkIndex];
                    previewMeshFilter = previewMeshObject.GetComponent<MeshFilter>();
                }
                else
                {
                    previewMeshObject = new GameObject("Terrain Preview Chunk")
                    {
                        tag = "PreviewChunk"
                    };
                    previewChunks.Add(previewMeshObject);
                    previewMeshObject.AddComponent<HideOnPlay>();
                    MeshRenderer previewMeshRenderer = previewMeshObject.AddComponent<MeshRenderer>();
                    previewMeshRenderer.sharedMaterial = terrainMaterial;
                    previewMeshFilter = previewMeshObject.AddComponent<MeshFilter>();
                }
                previewChunkIndex++;

                previewMeshObject.transform.position = new Vector3(position.x, 0, position.y);
                previewMeshObject.transform.SetParent(previewParent);

                MeshData meshData =
                    MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD);
                previewMeshFilter.sharedMesh = meshData.CreateMesh();

                CreateWaterPlane(previewMeshObject, position);
            }
        }

        if (previewChunkIndex < previewChunks.Count)
        {
            for (int i = previewChunks.Count - previewChunkIndex; i > 0; i--)
            {
                GameObject chunk = previewChunks[previewChunks.Count - 1];
                previewChunks.RemoveAt(previewChunks.Count - 1);
                DestroyImmediate(chunk);
            }
        }

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(false);
    }

    private void CreateWaterPlane(GameObject previewMeshObject, Vector2 position)
    {
        GameObject waterPlaneObject = null;
        if (previewMeshObject.transform.childCount > 0)
        {
            waterPlaneObject = previewMeshObject.transform.GetChild(0).gameObject;
            if (!heightMapSettings.useWaterPlane)
            {
                DestroyImmediate(waterPlaneObject);
            }
        }

        if (heightMapSettings.useWaterPlane)
        {
            if (waterPlaneObject == null)
            {
                waterPlaneObject = new GameObject("WaterPlane")
                {
                    layer = 11
                };
                waterPlaneObject.transform.SetParent(previewMeshObject.transform);

                MeshFilter waterPlaneMeshFilter = waterPlaneObject.AddComponent<MeshFilter>();
                waterPlaneMeshFilter.sharedMesh = waterMesh;
                MeshCollider waterPlaneMeshCollider = waterPlaneObject.AddComponent<MeshCollider>();
                waterPlaneMeshCollider.sharedMesh = waterMesh;
                MeshRenderer waterPlaneMeshRenderer = waterPlaneObject.AddComponent<MeshRenderer>();
                waterPlaneMeshRenderer.sharedMaterial = heightMapSettings.waterMaterial;
            }

            // set height and scale for water plane
            float waterHeight = heightMapSettings.heightCurve.Evaluate(heightMapSettings.waterHeight) *
                                heightMapSettings.heightMultiplier;
            waterPlaneObject.transform.position = new Vector3(previewMeshObject.transform.position.x, waterHeight, previewMeshObject.transform.position.z);
        }
    }

    public void DrawTexture(Texture2D texture, int gridSize)
    {
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(-meshSettings.meshWorldSize * gridSize, 1, meshSettings.meshWorldSize * gridSize) / 10f;

        textureRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    public void DrawMesh(MeshData meshData)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();

        textureRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    void OnValuesUpdated()
    {
        UpdateTerrainDataVariables();

        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    void UpdateDataCallbacks()
    {
        if (terrainDataPackage != null)
        {
            terrainDataPackage.OnValuesUpdated -= OnValuesUpdated;
            terrainDataPackage.OnValuesUpdated += OnValuesUpdated;
        }
        if (meshSettings != null)
        {
            meshSettings.OnValuesUpdated -= OnValuesUpdated;
            meshSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (heightMapSettings != null)
        {
            heightMapSettings.OnValuesUpdated -= OnValuesUpdated;
            heightMapSettings.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    void OnValidate()
    {
        UpdateDataCallbacks();
    }
}
