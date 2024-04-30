using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Ekkam
{
    public class TerrainGenerator : MonoBehaviour
{
    public Material material;
    public Vector2Int gridSize = new Vector2Int(3, 3);
    
    public Texture2D heightMap;
    public float heightMultiplier = 1f;
    
    public Color lowestColor;
    public Color highestColor;
    
    public Vector2 uvAnimationRate = new Vector2(0.1f, 0.1f);
    private Vector2 uvOffset = Vector2.zero;
    
    public bool staticBoi = false;
    public bool addCollider = false;

    void Start()
    {
        MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
        Mesh terrainMesh = GenerateGridMesh(gridSize.x, gridSize.y);
        meshFilter.mesh = terrainMesh;
        
        ApplyHeightMap(terrainMesh);
    }
    
    void Update()
    {
        if (staticBoi) return;
        
        // if (Input.GetKeyDown(KeyCode.M))
        // {
        //     MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        //     Mesh terrainMesh = meshFilter.mesh;
        //     ApplyHeightMap(terrainMesh);
        // }
        
        uvOffset += (uvAnimationRate * Time.deltaTime);
        MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
        Mesh terrainMesh = meshFilter.mesh;
        ApplyHeightMap(terrainMesh);
    }

    Mesh GenerateGridMesh(int width, int height)
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                vertices[x + y * (width + 1)] = new Vector3(x, 0, y);
            }
        }

        int[] triangles = new int[width * height * 6];
        for (int ti = 0, vi = 0, y = 0; y < height; y++, vi++)
        {
            for (int x = 0; x < width; x++, ti += 6, vi++)
            {
                // 0---1---2
                // | A | B |
                // 3---4---5
                // | C | D |
                // 6---7---8
                
                // First quad A is formed by triangles 0, 3, 1 and 1, 3, 4
                
                triangles[ti] = vi;
                triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                triangles[ti + 4] = triangles[ti + 1] = vi + width + 1;
                triangles[ti + 5] = vi + width + 2;
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        return mesh;
    }
    
    void ApplyHeightMap(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];
        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0, y = 0; y <= gridSize.y; y++)
        {
            for (int x = 0; x <= gridSize.x; x++, i++)
            {
                float uvX = (x / (float)gridSize.x) * heightMap.width + uvOffset.x;
                float uvY = (y / (float)gridSize.y) * heightMap.height + uvOffset.y;
                
                float height = heightMap.GetPixel((int)uvX, (int)uvY).grayscale * heightMultiplier;

                vertices[i].y = height;
                uvs[i] = new Vector2(x / (float)gridSize.x, y / (float)gridSize.y);
                colors[i] = Color.Lerp(lowestColor, highestColor, height / heightMultiplier);
            }
        }

        mesh.vertices = vertices;
        mesh.colors = colors;
        mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        if (addCollider)
        {
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
        }
    }

}
}