﻿using System.Collections;
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

    private bool generateFixedSizeTerrain;
    private int fixedTerrainSize;
    private int numVertsPerLine;
    private float[,] falloffMap;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    void Start()
    {
        textureSettings.ApplyToMaterial(mapMaterial);
        textureSettings.UpdateMeshHeights(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDist = detailLevels[detailLevels.Length - 1].visibleDistThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDist / meshWorldSize);

        generateFixedSizeTerrain = meshSettings.generateFixedSizeTerrain;
        fixedTerrainSize = meshSettings.fixedTerrainSize;
        numVertsPerLine = meshSettings.numVertsPerLine;

        if (heightMapSettings.useFalloff)
            falloffMap = FalloffGenerator.GenerateFalloffMap(numVertsPerLine * ((fixedTerrainSize * 2) + 1));

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
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();

        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        if (generateFixedSizeTerrain)
        {
            for (int y = -fixedTerrainSize; y <= fixedTerrainSize; y++)
            {
                for (int x = -fixedTerrainSize; x <= fixedTerrainSize; x++)
                {
                    UpdateChunk(x, y, alreadyUpdatedChunkCoords);
                }
            }
        }
        else
        {
            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

            for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
            {
                for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
                {
                    UpdateChunk(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset, alreadyUpdatedChunkCoords);
                }
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
                TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels,
                    colliderLODIndex, transform, viewer, mapMaterial);

                if (generateFixedSizeTerrain && heightMapSettings.useFalloff)
                    newChunk.UseFalloffMap(falloffMap, numVertsPerLine * (x + fixedTerrainSize), numVertsPerLine * (y + fixedTerrainSize));

                terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                newChunk.Load();
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
