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

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureData;

    public Material terrainMaterial;

    public Transform previewParent;

    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int editorPreviewLOD;

    public bool previewWholeFixedSizeMesh;

    public bool autoUpdate;

    private List<GameObject> previewChunks;


    public void DrawMapInEditor()
    {
        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        int previewSize = meshSettings.numVertsPerLine;
        int gridSize = 1;

        if (meshSettings.generateFixedSizeTerrain)
        {
            gridSize = meshSettings.fixedTerrainSize * 2 + 1;
            previewSize = (meshSettings.numVertsPerLine - 2) * gridSize + 2;
        }

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
                DestroyImmediate(chunk);
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
                    int falloffStartX = (meshSettings.numVertsPerLine - 2) * (x + fixedTerrainSize);
                    int falloffStartY = previewSize - 2 - (meshSettings.numVertsPerLine - 2) * (y + fixedTerrainSize + 1);

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
        if (!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
    }

    void OnValidate()
    {
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
}
