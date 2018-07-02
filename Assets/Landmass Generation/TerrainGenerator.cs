using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    private const float viewerMoveThresholdForChunkUpdate = 25f;
    private const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;

    public Transform viewer;
    public Material mapMaterial;

    private Vector2 viewerPosition;
    private Vector2 viewerPositionOld;

    float meshWorldSize;
    int chunksVisibleInViewDst;

    // set size generation
    public bool generateFixedSizeTerrain;
    public int fixedTerrainSize;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDist / meshWorldSize);

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

    void UpdateVisibleChunks()
    {
        if (generateFixedSizeTerrain)
        {
            HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

            for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
                visibleTerrainChunks[i].UpdateTerrainChunk();
            }

            for (int y = -fixedTerrainSize; y < fixedTerrainSize; y++)
            {
                for (int x = -fixedTerrainSize; x < fixedTerrainSize; x++)
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
                            TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels,
                                colliderLODIndex, transform, viewer, mapMaterial);
                            terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                            newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                            newChunk.Load();
                        }
                    }
                }
            }
        }
        else
        {
            HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

            for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
            {
                alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
                visibleTerrainChunks[i].UpdateTerrainChunk();
            }

            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

            for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
            {
                for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                    if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                    {
                        if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                        {
                            terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                        }
                        else
                        {
                            TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels,
                                colliderLODIndex, transform, viewer, mapMaterial);
                            terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                            newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                            newChunk.Load();
                        }
                    }
                }
            }
        }
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
