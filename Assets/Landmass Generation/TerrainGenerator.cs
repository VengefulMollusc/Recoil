using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public TerrainDataPackage terrainDataPackage;
    private HeightMapSettings heightMapSettings;
    private MeshSettings meshSettings;
    private TextureData textureSettings;
    private TerrainPopulationSettings populationSettings;

    public Transform viewer;
    public Material mapMaterial;

    private Vector2 viewerPosition;
    private Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDst;

    private bool generateFixedSizeTerrain;
    private int fixedTerrainSize;
    private int numVertsPerLine;
    private int falloffMapSize;
    private float[,] falloffMap;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start()
    {
        UpdateTerrainDataVariables();

        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDist / meshWorldSize);

        generateFixedSizeTerrain = meshSettings.generateFixedSizeTerrain;

        if (generateFixedSizeTerrain)
        {
            fixedTerrainSize = meshSettings.fixedTerrainSize;
            numVertsPerLine = meshSettings.numVertsPerLine;
            falloffMapSize = (numVertsPerLine - 3) * (fixedTerrainSize * 2 + 1) + 3;

            if (heightMapSettings.useFalloff)
                falloffMap = FalloffGenerator.GenerateFalloffMap(falloffMapSize, heightMapSettings.falloffMode);
        }

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionOld)
        {
            foreach (TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateTerrainDataVariables()
    {
        heightMapSettings = terrainDataPackage.heightMapSettings;
        meshSettings = terrainDataPackage.meshSettings;
        textureSettings = terrainDataPackage.textureData;
        populationSettings = terrainDataPackage.populationSettings;
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        if (IsCoordOutOfBounds(currentChunkCoordX, currentChunkCoordY))
        {
            // TODO: activate oob effects etc for player
        }

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                UpdateChunk(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset, alreadyUpdatedChunkCoords);
            }
        }
    }

    void UpdateChunk(int x, int y, HashSet<Vector2> alreadyUpdatedChunkCoords)
    {
        Vector2 viewedChunkCoord = new Vector2(x, y);
        if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
        {
            if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
            {
                terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
            }
            else
            {
                bool useGlobalFalloff = generateFixedSizeTerrain && heightMapSettings.useFalloff;
                bool isOutOfBounds = IsCoordOutOfBounds(x, y);
                bool isFlatChunk = meshSettings.flattenOutsideTerrain && useGlobalFalloff && isOutOfBounds;

                // flatten LODs to lowest when using flat planes
                LODInfo[] lods = !isFlatChunk ? detailLevels : new[] { detailLevels[detailLevels.Length - 1] };
                TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, lods,
                    colliderLODIndex, transform, viewer, mapMaterial, populationSettings);

                terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;

                if (useGlobalFalloff && !isFlatChunk)
                {
                    int falloffStartX = (numVertsPerLine - 3) * (x + fixedTerrainSize);
                    int falloffStartY = falloffMapSize - 3 - (numVertsPerLine - 3) * (y + fixedTerrainSize + 1);

                    newChunk.Load(falloffMap, falloffStartX, falloffStartY);
                }
                else
                {
                    newChunk.Load(isOutOfBounds, isFlatChunk);
                }
            }
        }
    }

    bool IsCoordOutOfBounds(int x, int y)
    {
        return generateFixedSizeTerrain && (x < -fixedTerrainSize || x > fixedTerrainSize ||
                                            y < -fixedTerrainSize || y > fixedTerrainSize);
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }

}

[System.Serializable]
public struct LODInfo
{
    [Range(0, MeshSettings.numSupportedLODs - 1)]
    public int lod;
    public float visibleDistThreshold;

    public float sqrVisibleDistanceThreshold
    {
        get { return visibleDistThreshold * visibleDistThreshold; }
    }
}
