using System.Collections;
using UnityEngine;

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}

[RequireComponent(typeof(MeshFilter))]
public class MeshGeneration : MonoBehaviour
{
    public Mesh mymesh;
    public TerrainType[] terrains;

    Vector3[] vertices;
    Vector2[] uvs;
    int[] triangles;

    public int XmapSize = 10;
    public int ZmapSize = 10;
    public float noiseScale = 0.2f;
    //Controls increase in frequency of octaves
    [Range(0, 5)] public float lacunarity = 0.2f;

    [Range(0, 1)] public float persistance = 0.2f;

    [Range(1,10)] public int octaves = 2;

    [Range(1, 20)] public float terrainHeight = 10;
    public bool AutoUpdate = false;

    public float perlinNoise = 0f;
    public AnimationCurve heightCurve;
    private float[,] noiseMap;


    [SerializeField] private int triangleIndex = 0;

    private int vertIndex = 0;

    void Awake()
    {
        SetMeshData();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreateShapeCouroutine());
    }

    void Update()
    {
        UpdateMesh();
    }

    public IEnumerator CreateShapeCouroutine()
    {
        ////triangles = new int[(XmapSize - 1) * (ZmapSize - 1) * 6];

        //int verticesIndex = 0;

        //for (int z = 0; z < ZmapSize; z++)
        //{
        //    for (int x = 0; x < XmapSize ; x++)
        //    {
        //        perlinNoise = PerlinNoise.Perlin(x * 0.1f, z * 0.1f, 0);
        //        vertices[verticesIndex] = new Vector3(x,/*perlinNoise * 20*/0,z);
        //        if (triangleIndex < triangles.Length)
        //        {
        //            //CreateTriangle(verticesIndex + 1, verticesIndex, verticesIndex + XmapSize, ref triangleIndex);
        //            //CreateTriangle(verticesIndex + 1, verticesIndex + XmapSize, verticesIndex + XmapSize + 1, ref triangleIndex);
        //        }
        //        verticesIndex++;
        //        yield return new WaitForSeconds(0.4f);
        //    }
        //}

        int verticesIndex = 0;

        for (int z = 0; z <= ZmapSize; z++)
        {
            for (int x = 0; x <= XmapSize; x++)
            {
                vertices[verticesIndex] = new Vector3(x, noiseMap[x,z] * terrainHeight, z);
                uvs[verticesIndex] = new Vector2(x / (float)XmapSize, z / (float)ZmapSize);
                verticesIndex++;
            }
        }

        //if (triangleIndex < triangles.Length)
        //{
        //    //CreateTriangle(vertIndex + 1, vertIndex, vertIndex + XmapSize);
        //    //CreateTriangle(vertIndex + 1, vertIndex + XmapSize, vertIndex + XmapSize + 1);
        //    CreateQuad(XmapSize);

        //}

        for (int z = 0; z < ZmapSize; z++)
        {
            for (int x = 0; x < XmapSize; x++)
            {
                if (triangleIndex < triangles.Length)
                {
                    CreateQuad(XmapSize);
                    yield return new WaitForSeconds(0.00001f);
                }
            }
            vertIndex++;
        }
    }

    public void CreateShape()
    {
        //triangles = new int[(XmapSize - 1) * (ZmapSize - 1) * 6];

        int verticesIndex = 0;

        for (int z = 0; z <= ZmapSize; z++)
        {
            for (int x = 0; x <= XmapSize; x++)
            {
                vertices[verticesIndex] = new Vector3(x, heightCurve.Evaluate(noiseMap[x, z]) * terrainHeight, z);
                uvs[verticesIndex] = new Vector2(x/(float)XmapSize, z/(float)ZmapSize);
                verticesIndex++;
            }
        }

        //if (triangleIndex < triangles.Length)
        //{
        //    //CreateTriangle(vertIndex + 1, vertIndex, vertIndex + XmapSize);
        //    //CreateTriangle(vertIndex + 1, vertIndex + XmapSize, vertIndex + XmapSize + 1);
        //    CreateQuad(XmapSize);

        //}

        for (int z = 0; z < ZmapSize; z++)
        {
            for (int x = 0; x < XmapSize; x++)
            {
                if (triangleIndex < triangles.Length)
                {
                    CreateQuad(XmapSize);
                }
            }

            vertIndex++;
        }

    }

    public void UpdateMesh()
    {
        mymesh.Clear();
        mymesh.vertices = vertices;
        mymesh.triangles = triangles;
        mymesh.uv = uvs;
        mymesh.RecalculateNormals();
    }

    void CreateQuad(int xMapSize)
    {
        //triangles[triangleIndex + 0] = vertIndex;
        //triangles[triangleIndex + 1] = vertIndex + XmapSize;
        //triangles[triangleIndex + 2] = vertIndex + 1;

        //triangles[triangleIndex + 3] = vertIndex + 1;
        //triangles[triangleIndex + 4] = vertIndex + XmapSize;
        //triangles[triangleIndex + 5] = vertIndex + XmapSize + 1;

        triangles[triangleIndex + 0] = vertIndex;
        triangles[triangleIndex + 1] = vertIndex + XmapSize + 1;
        triangles[triangleIndex + 2] = vertIndex + 1;

        triangles[triangleIndex + 3] = vertIndex + 1;
        triangles[triangleIndex + 4] = vertIndex + XmapSize + 1;
        triangles[triangleIndex + 5] = vertIndex + XmapSize + 2;

        vertIndex++;
        triangleIndex += 6;
    }

    public void SetMeshData()
    {
        mymesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mymesh;
        triangleIndex = 0;
        vertIndex = 0;
        noiseMap = MapGeneration.GenerateNoiseMap(XmapSize, ZmapSize, noiseScale, lacunarity, persistance, octaves);
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture =
            TextureGeneration.ColourMap(XmapSize, ZmapSize, terrains, noiseMap);
        //Fix: in the vertices I had to increment mapSize + 1 for each
        //10 Square and 11 vertices!
        vertices = new Vector3[(XmapSize + 1) * (ZmapSize + 1)];
        triangles = new int[(XmapSize) * (ZmapSize) * 6];
        uvs = new Vector2[(XmapSize + 1) * (ZmapSize + 1)];
    }


    //void OnDrawGizmos()
    //{
    //    if (vertices == null)
    //        return;

    //    for (int i = 0; i < vertices.Length; i++)
    //    {
    //        Gizmos.DrawSphere(vertices[i], .1f);
    //    }
    //}
}
