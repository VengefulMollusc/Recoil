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


    public void DrawMapInEditor()
    {
        foreach (GameObject chunk in GameObject.FindGameObjectsWithTag("PreviewChunk"))
        {
            DestroyImmediate(chunk);
        }

        textureData.ApplyToMaterial(terrainMaterial);
        textureData.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        int previewSize = meshSettings.numVertsPerLine;
        int gridSize = 1;

        if (meshSettings.generateFixedSizeTerrain)
        {
            gridSize = meshSettings.fixedTerrainSize * 2 + 1;
            previewSize = meshSettings.numVertsPerLine * gridSize;
        }

        if (drawMode == DrawMode.NoiseMap)
        {
            float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(previewSize);
            HeightMap heightMap = (heightMapSettings.useFalloff) ? HeightMapGenerator.GenerateHeightMapWithFalloff(previewSize, heightMapSettings, Vector2.zero, falloffMap, 0, 0) :
                HeightMapGenerator.GenerateHeightMap(previewSize, heightMapSettings, Vector2.zero);
            DrawTexture(TextureGenerator.TextureFromHeightMap(heightMap), gridSize);
        }
        else if (drawMode == DrawMode.DrawMesh)
        {
            if (previewWholeFixedSizeMesh && meshSettings.generateFixedSizeTerrain)
            {
                int fixedTerrainSize = meshSettings.fixedTerrainSize;
                float[,] falloffMap = FalloffGenerator.GenerateFalloffMap(previewSize);
                for (int x = -fixedTerrainSize; x <= fixedTerrainSize; x++)
                {
                    for (int y = -fixedTerrainSize; y <= fixedTerrainSize; y++)
                    {
                        Vector2 position = new Vector2(x * meshSettings.meshWorldSize, y * meshSettings.meshWorldSize);
                        Vector2 sampleCenter = position / meshSettings.meshScale;
                        HeightMap heightMap;

                        if (heightMapSettings.useFalloff)
                        {
                            int falloffStartX = meshSettings.numVertsPerLine * (x + fixedTerrainSize);
                            int falloffStartY = previewSize -
                                                (meshSettings.numVertsPerLine * (y + fixedTerrainSize + 1));

                            heightMap = HeightMapGenerator.GenerateHeightMapWithFalloff(meshSettings.numVertsPerLine, heightMapSettings,
                                    sampleCenter, falloffMap, falloffStartX, falloffStartY);
                        }
                        else
                        {
                            heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, heightMapSettings,
                                    sampleCenter);
                        }
                        

                        GameObject previewMeshObject = new GameObject("Terrain Preview Chunk")
                        {
                            tag = "PreviewChunk"
                        };
                        previewMeshObject.AddComponent<HideOnPlay>();
                        MeshRenderer previewMeshRenderer = previewMeshObject.AddComponent<MeshRenderer>();
                        previewMeshRenderer.sharedMaterial = terrainMaterial;
                        MeshFilter previewMeshFilter = previewMeshObject.AddComponent<MeshFilter>();

                        previewMeshObject.transform.position = new Vector3(position.x, 0, position.y);
                        previewMeshObject.transform.SetParent(previewParent);

                        MeshData meshData =
                            MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD);
                        previewMeshFilter.sharedMesh = meshData.CreateMesh();
                    }
                }

                textureRenderer.gameObject.SetActive(false);
                meshFilter.gameObject.SetActive(false);
            }
            else
            {
                HeightMap heightMap = HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, heightMapSettings, Vector2.zero);
                DrawMesh(MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, editorPreviewLOD));
            }
        }
        else if (drawMode == DrawMode.FalloffMap)
        {
            DrawTexture(TextureGenerator.TextureFromHeightMap(new HeightMap(FalloffGenerator.GenerateFalloffMap(previewSize), 0, 1)), gridSize);
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
