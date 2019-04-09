using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;
using Random = System.Random;

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

    public int mapSize = 255;

    [Range(0,4)]
    public int LevelOfDetail = 0;

    public float noiseScale = 0.2f;
    //Controls increase in frequency of octaves
    [Range(0, 5)] public float lacunarity = 0.2f;

    [Range(0, 1)] public float persistance = 0.2f;

    [Range(1,10)] public int octaves = 2;

    [Range(1, 50)] public float terrainHeight = 10;
    public bool AutoUpdate = false;

    public float perlinNoise = 0f;
    public AnimationCurve heightCurve;
    private float[] noiseMap;


    [SerializeField] private int triangleIndex = 0;

    private int vertIndex = 0;

    //Sliders
    public Slider[] sliders;

    //SingleTon
    public static MeshGeneration instance;

    private test2 erosion;

    public int iterations = 30;

    void Awake()
    {
        SetMeshData();
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(CreateShapeCouroutine());
        sliders[0].onValueChanged.AddListener(delegate { lacunarity = sliders[0].value;});
        sliders[1].onValueChanged.AddListener(delegate { persistance = sliders[1].value;});
        sliders[2].onValueChanged.AddListener(delegate { octaves = (int)sliders[2].value;});
        sliders[3].onValueChanged.AddListener(delegate { terrainHeight = sliders[3].value;});
    }
    public IEnumerator CreateShapeCouroutine()
    {
        ////triangles = new int[(mapSize - 1) * (mapSize - 1) * 6];

        //int verticesIndex = 0;

        //for (int z = 0; z < mapSize; z++)
        //{
        //    for (int x = 0; x < mapSize ; x++)
        //    {
        //        perlinNoise = PerlinNoise.Perlin(x * 0.1f, z * 0.1f, 0);
        //        vertices[verticesIndex] = new Vector3(x,/*perlinNoise * 20*/0,z);
        //        if (triangleIndex < triangles.Length)
        //        {
        //            //CreateTriangle(verticesIndex + 1, verticesIndex, verticesIndex + mapSize, ref triangleIndex);
        //            //CreateTriangle(verticesIndex + 1, verticesIndex + mapSize, verticesIndex + mapSize + 1, ref triangleIndex);
        //        }
        //        verticesIndex++;
        //        yield return new WaitForSeconds(0.4f);
        //    }
        //}

        int verticesIndex = 0;

        for (int z = 0; z <= mapSize; z++)
        {
            for (int x = 0; x <= mapSize; x++)
            {
                vertices[verticesIndex] = new Vector3(x, noiseMap[z*mapSize + x] * terrainHeight, z);
                uvs[verticesIndex] = new Vector2(x / (float)mapSize, z / (float)mapSize);
                verticesIndex++;
            }
        }

        //if (triangleIndex < triangles.Length)
        //{
        //    //CreateTriangle(vertIndex + 1, vertIndex, vertIndex + mapSize);
        //    //CreateTriangle(vertIndex + 1, vertIndex + mapSize, vertIndex + mapSize + 1);
        //    CreateQuad(mapSize);

        //}

        for (int z = 0; z < mapSize; z++)
        {
            for (int x = 0; x < mapSize; x++)
            {
                if (triangleIndex < triangles.Length)
                {
                    CreateQuad(mapSize);
                    yield return new WaitForSeconds(0.00001f);
                }
            }
            vertIndex++;
        }
    }
    void Update()
    {
        UpdateMesh();
    }


    public void SetMeshData()
    {
        mymesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mymesh;
        triangleIndex = 0;
        vertIndex = 0;
        noiseMap = MapGeneration.GenerateNoiseMap(mapSize, noiseScale, lacunarity, persistance, octaves);
        //GetComponent<MeshRenderer>().sharedMaterial.mainTexture = TextureGeneration.ColourMap(mapSize, mapSize, terrains, noiseMap);
        //GetComponent<MeshRenderer>().material = cle
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = null;
        GetComponent<MeshRenderer>().sharedMaterial.color = Color.grey;
        //Fix: in the vertices I had to increment mapSize + 1 for each
        //10 Square and 11 vertices!
        vertices = new Vector3[(mapSize + 1) * (mapSize + 1)];
        triangles = new int[(mapSize) * (mapSize) * 6];
        uvs = new Vector2[(mapSize + 1) * (mapSize + 1)];
    }
    public void CreateShape()
    {
        //triangles = new int[(mapSize - 1) * (mapSize - 1) * 6];

        int lod = 0;
        switch (LevelOfDetail)
        {
            case 0:
                lod = 1;
                break;
            case 1:
                lod = 3;
                break;
            case 2:
                lod = 5;
                break;
            case 3:
                lod = 15;
                break;
            case 4:
                lod = 17;
                break;
        }


    int verticesperline = (mapSize / lod);
        //Sets the data for the Mesh, Vertices and Triangles
        UpdateMeshData(verticesperline);
        int verticesIndex = 0;

        for (int y = 0; y <= mapSize; y+= lod)
        {
            for (int x = 0; x <= mapSize; x+= lod)
            {
                vertices[verticesIndex] = new Vector3(x, heightCurve.Evaluate(noiseMap[y * mapSize + x]) * terrainHeight, y);
                uvs[verticesIndex] = new Vector2(x/(float)mapSize, y/(float)mapSize);
                verticesIndex++;
            }
        }

        //if (triangleIndex < triangles.Length)
        //{
        //    //CreateTriangle(vertIndex + 1, vertIndex, vertIndex + mapSize);
        //    //CreateTriangle(vertIndex + 1, vertIndex + mapSize, vertIndex + mapSize + 1);
        //    CreateQuad(mapSize);

        //}

        for (int z = 0; z < mapSize; z+= lod)
        {
            for (int x = 0; x < mapSize; x+= lod)
            {
                if (triangleIndex < triangles.Length)
                {
                    CreateQuad(verticesperline);
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

    public void UpdateMeshData(int x)
    {
        triangleIndex = 0;
        vertIndex = 0;
        //Fix: in the vertices I had to increment mapSize + 1 for each
        //10 Square and 11 vertices!
        vertices = new Vector3[(x + 1) * (x + 1)];
        triangles = new int[(x) * (x) * 6];
        uvs = new Vector2[(x + 1) * (x + 1)];
    }
    void CreateQuad(int xMapSize)
    {
        //triangles[triangleIndex + 0] = vertIndex;
        //triangles[triangleIndex + 1] = vertIndex + mapSize;
        //triangles[triangleIndex + 2] = vertIndex + 1;

        //triangles[triangleIndex + 3] = vertIndex + 1;
        //triangles[triangleIndex + 4] = vertIndex + mapSize;
        //triangles[triangleIndex + 5] = vertIndex + mapSize + 1;

        triangles[triangleIndex + 0] = vertIndex;
        triangles[triangleIndex + 1] = vertIndex + xMapSize + 1;
        triangles[triangleIndex + 2] = vertIndex + 1;

        triangles[triangleIndex + 3] = vertIndex + 1;
        triangles[triangleIndex + 4] = vertIndex + xMapSize + 1;
        triangles[triangleIndex + 5] = vertIndex + xMapSize + 2;

        vertIndex++;
        triangleIndex += 6;
    }
    public void Erode()
    {
        noiseMap = MapGeneration.GenerateNoiseMap(mapSize, noiseScale, lacunarity, persistance, octaves);
        erosion = FindObjectOfType<test2>();
        erosion.Erode(noiseMap, mapSize, iterations);
        CreateShape();
        UpdateMesh();
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
