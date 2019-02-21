using UnityEngine;

public class newMeshGenerator : MonoBehaviour
{
    public int MeshWidth;
    public int MeshHeight;

    private Vector3[] vertices;
    private int[] triangles;
    private int triangleIndex;

    void CalculateMesh(int width, int height)
    {
        vertices = new Vector3[width * height];
        triangles = new int[(width-1)*(height-1)*6];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                
            }
        }
    }

    void AddTriangles(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        return mesh;
    }

}
