using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterPlaneGenerator : MonoBehaviour
{
    [Header("Plane Variables")]
    public float size = 1f;
    public int gridSize = 16;

    [Header("Noise Variables")]
    public float power = 3f;
    public float scale = 1f;
    public float timeScale = 1f;

    private float xOffset;
    private float yOffset;

    private MeshFilter meshFilter;

    // Use this for initialization
    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = GenerateMesh();
        
        ApplyWaterNoise();
    }

    // Update is called once per frame
    void Update()
    {
        ApplyWaterNoise();
        xOffset += Time.deltaTime * timeScale;
        yOffset += Time.deltaTime * timeScale;
    }

    private Mesh GenerateMesh()
    {
        Mesh mesh = new Mesh();

        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        for (int x = 0; x <= gridSize; x++)
        {
            for (int y = 0; y <= gridSize; y++)
            {
                vertices.Add(new Vector3(-size * 0.5f + size * (x / (float)gridSize), 0, -size * 0.5f + size * (y / (float)gridSize)));
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(x / (float)gridSize, y / (float)gridSize));
            }
        }

        List<int> triangles = new List<int>();
        int vertCount = gridSize + 1;

        for (int i = 0; i < vertCount * vertCount - vertCount; i++)
        {
            if ((i + 1) % vertCount == 0)
            {
                continue;
            }
            triangles.AddRange(new List<int>()
            {
                i + 1 + vertCount, i + vertCount, i,
                i, i + 1, i + 1 + vertCount
            });
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);

        return mesh;
    }

    void ApplyWaterNoise()
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
