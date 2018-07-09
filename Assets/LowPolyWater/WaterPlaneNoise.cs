using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPlaneNoise : MonoBehaviour
{
    public float power = 3f;
    public float scale = 1f;
    public float timeScale = 1f;

    private float xOffset;
    private float yOffset;
    private MeshFilter meshFilter;

    // Use this for initialization
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        MakeNoise();
    }

    // Update is called once per frame
    void Update()
    {
        MakeNoise();
        xOffset += Time.deltaTime * timeScale;
        yOffset += Time.deltaTime * timeScale;
    }

    void MakeNoise()
    {
        Vector3[] vertices = meshFilter.mesh.vertices;

        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i].y = CalculateHeight(vertices[i].x, vertices[i].z) * power;
        }

        meshFilter.mesh.vertices = vertices;
    }

    float CalculateHeight(float x, float y)
    {
        float xCoord = x * scale + xOffset;
        float yCoord = y * scale + yOffset;

        return Mathf.PerlinNoise(xCoord, yCoord);
    }
}
