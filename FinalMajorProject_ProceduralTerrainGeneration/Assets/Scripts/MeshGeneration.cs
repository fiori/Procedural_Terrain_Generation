using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

namespace Assets.Scripts
{
    [System.Serializable]
    public struct TerrainType
    {
        public string name;
        /// Noise Map
        public float height;
        /// Terrain Colour
        public Color colour;
    }

    [RequireComponent(typeof(MeshFilter),typeof(Erosion))]
    public class MeshGeneration : MonoBehaviour
    {
        //////////////////////////////////////////////////////////////////////////////////
        //Private Variables
        ///Quantity of vertices present on the mesh 
        private Vector3[] _vertices;

        ///The mesh UVS go from [0 - 1] in mesh so to get the right UVS currentMapPosition / mapSize
        private Vector2[] _uvs;

        ///Each triangle is composed by 3 vertices where each quad is composed by 6 vertices 
        private int[] _triangles;

        ///This variable is initialized in the method SetMeshData() and gets the value from MapGeneration.GenerateNoiseMap()
        private float[] _noiseMap;

        ///Shows how many triangles are present in the mesh.
        [SerializeField] private int _vertexIndex;

        ///This variable is used to count in what Y row the triangle is going to be drawn.  
        private int _rowIndex;

        ///The initialization for this variable is made by using FindObjectOfType<Erosion> and add the reference from the attached script to this variable.  
        private Erosion erosion;
        
        //////////////////////////////////////////////////////////////////////////////////
        //Public Variables
        /// Mesh is an empty gameObject from UnityEngine nameSpace
        public Mesh MyMesh;

        ///TerrainType Array, this array is used to define if the terrain is water,sand etc...
        public TerrainType[] terrains;

        /// Readonly map size set to 255 
        public readonly int MapSize = 255;

        /// Size of the Noise Map, the bigger the value, bigger the frequency of the noise giving a zoomed effect on the map.
        public float NoiseScale = 20.0f;

        /// Used for reducing the level of detail of the mesh, the higher the level the less vertices the mesh is going to have
        [Range(0,4)] public int LevelOfDetail = 0;

        ///Controls the increase in frequency of a octave, the width of the wave
        [Range(0, 5)] public float Lacunarity = 3.00f;

        ///Controls the increase in amplitude of a octave, the height of the wave
        [Range(0, 1)] public float Persistance = 0.3f;

        ///Octave is the number of level of detail for the Perlin noise
        [Range(1,10)] public int Octaves = 6;

        ///This value multiplies the noiseMap value to a bigger and noticeable value. The value of the multiplication then is set to the Y coordinate of the vertex in the mesh.
        [Range(1, 50)] public float TerrainHeight = 25.0f;
        
        ///A boolean used in the MeshEditor, when this value is true, it updates the mesh every time a change is made
        public bool AutoUpdate = false;

        ///This AnimationCurve is used when setting the Y value to the vertices https://docs.unity3d.com/Manual/animeditor-AnimationCurves.html
        public AnimationCurve HeightCurve;

        ///Slider[] comes from UnityEngine.UI and is a part of the User Interface of the program, this sliders are responsible to change the terrain generation.  
        public Slider[] Sliders;

        ///This is a design pattern named singleton. Singleton allows the access for this class in all the project solution.  
        public static MeshGeneration Instance;

        ///How many water particles to be generated to create erosion. 
        public int Iterations = 150000;

        private float Rotation = 0;

        /// <summary>
        /// Sets the Mesh Data and add the singleton instance to this script.
        /// </summary>
        void Awake()
        {
            SetMeshData();
            Instance = this;
        }

        /// <summary>
        /// Starts the Coroutine method, sets the value to the sliders and Adds a listener to each of the sliders to check for value change.
        /// </summary>
        void Start()
        {
            var iterationText = GameObject.Find("TextNumberOfIterations").GetComponent<Text>();

            StartCoroutine(CreateShapeCoroutine());
            Sliders[0].value = Lacunarity;
            Sliders[1].value = Persistance;
            Sliders[2].value = Octaves;
            Sliders[3].value = TerrainHeight;
            Sliders[4].value = Iterations;
            iterationText.text = Iterations.ToString();
            Sliders[0].onValueChanged.AddListener(delegate { Lacunarity = Sliders[0].value;});
            Sliders[1].onValueChanged.AddListener(delegate { Persistance = Sliders[1].value;});
            Sliders[2].onValueChanged.AddListener(delegate { Octaves = (int)Sliders[2].value;});
            Sliders[3].onValueChanged.AddListener(delegate { TerrainHeight = Sliders[3].value;});
            Sliders[4].onValueChanged.AddListener(delegate { Iterations = (int)Sliders[4].value;
                iterationText.text = Iterations.ToString();});
        }

        /// <summary>
        /// Uses the Coroutine to wait a second between each quad generated.
        /// So the user can see the terrain being generated each frame.
        /// </summary>
        /// <returns>Returns the IEnumerator that is the amount of seconds to wait before the next Coroutine</returns>
        public IEnumerator CreateShapeCoroutine()
        {
            int verticesIndex = 0;

            for (int z = 0; z <= MapSize; z++)
            {
                for (int x = 0; x <= MapSize; x++)
                {
                    _vertices[verticesIndex] = new Vector3(x, _noiseMap[z*MapSize + x] * TerrainHeight, z);
                    _uvs[verticesIndex] = new Vector2(x / (float)MapSize, z / (float)MapSize);
                    verticesIndex++;
                }
            }
            for (int z = 0; z < MapSize; z++)
            {
                for (int x = 0; x < MapSize; x++)
                {
                    if (_vertexIndex < _triangles.Length)
                    {
                        CreateQuad(MapSize);
                        yield return new WaitForSeconds(0.00001f);
                    }
                }
                _rowIndex++;
            }
        }

        /// <summary>
        /// Updates the Mesh every second.
        /// </summary>
        void Update()
        {
            UpdateMesh();
            Rotation += 0.1f;
            this.transform.parent.localRotation = Quaternion.Euler(new Vector3(this.transform.rotation.x,Rotation,this.transform.rotation.z));
        }

        /// <summary>
        /// Creates the mesh, sets the mesh filter to the mesh just created
        /// Generates the noiseMap and stores it inside the _noiseMap
        /// Adds a mainTexture or color using the component MeshRenderer to the new mesh we created
        /// Initializes the vertices,triangles and uvs.
        /// </summary>
        public void SetMeshData()
        {
            MyMesh = new Mesh();
            GetComponent<MeshFilter>().mesh = MyMesh;
            _vertexIndex = 0;
            _rowIndex = 0;
            _noiseMap = MapGeneration.GenerateNoiseMap(MapSize, NoiseScale, Lacunarity, Persistance, Octaves);
            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = null;
            //GetComponent<MeshRenderer>().sharedMaterial.mainTexture = TextureGeneration.ColourMap(mapSize, terrains, noiseMap);

            //GetComponent<MeshRenderer>().sharedMaterial.color = Color.gray;
            //Fix: in the vertices I had to increment mapSize + 1 for each
            //10 Square and 11 vertices!
            _vertices = new Vector3[(MapSize + 1) * (MapSize + 1)];
            _triangles = new int[(MapSize) * (MapSize) * 6];
            _uvs = new Vector2[(MapSize + 1) * (MapSize + 1)];
        }

        /// <summary>
        /// Checks for the level of details set by the user
        /// New variable named vertices per line is created (MapSize / LevelOfDetail)
        /// Calls the method UpdateMeshData() to set the new data for the mesh
        /// Fills the value of the _vertices[] & _uvs[]
        /// Creates each triangle until it reaches the end of the MapSize.
        /// </summary>
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


            int verticesPerLine = (MapSize / lod);
            //Sets the data for the Mesh, Vertices and Triangles
            UpdateMeshData(verticesPerLine);
            int verticesIndex = 0;

            for (int y = 0; y <= MapSize; y+= lod)
            {
                for (int x = 0; x <= MapSize; x+= lod)
                {
                    _vertices[verticesIndex] = new Vector3(x, HeightCurve.Evaluate(_noiseMap[y * MapSize + x]) * TerrainHeight, y);
                    _uvs[verticesIndex] = new Vector2(x/(float)MapSize, y/(float)MapSize);
                    verticesIndex++;
                }
            }
            for (int z = 0; z < MapSize; z+= lod)
            {
                for (int x = 0; x < MapSize; x+= lod)
                {
                    if (_vertexIndex < _triangles.Length)
                    {
                        CreateQuad(verticesPerLine);
                    }
                }

                _rowIndex++;
            }

        }

        /// <summary>
        /// Clears the current Mesh
        /// Sets the Mesh.vertices, triangles and uvs = to the ones generated in create UpdateMeshData() method.
        /// And to finish it calls the function RecaculateNormals() in the mesh.
        /// </summary>
        public void UpdateMesh()
        {
            MyMesh.Clear();
            MyMesh.vertices = _vertices;
            MyMesh.triangles = _triangles;
            MyMesh.uv = _uvs;
            MyMesh.RecalculateNormals();
        }

        /// <summary>
        /// This method is used due to the fact that the author implemented the level of detail in the mesh
        /// </summary>
        /// <param name="x">How many vertices exist per line; current(MapSize/LevelOfDetail)</param>
        public void UpdateMeshData(int x)
        {
            _vertexIndex = 0;
            _rowIndex = 0;
            //Fix: in the vertices I had to increment mapSize + 1 for each
            //10 Square and 11 vertices!
            _vertices = new Vector3[(x + 1) * (x + 1)];
            _triangles = new int[(x) * (x) * 6];
            _uvs = new Vector2[(x + 1) * (x + 1)];
        }

        /// <summary>
        /// This method creates two triangles making a quad.
        /// </summary>
        /// <param name="xMapSize">Vertices per line</param>
        void CreateQuad(int xMapSize)
        {
            _triangles[_vertexIndex + 0] = _rowIndex;
            _triangles[_vertexIndex + 1] = _rowIndex + xMapSize + 1;
            _triangles[_vertexIndex + 2] = _rowIndex + 1;

            _triangles[_vertexIndex + 3] = _rowIndex + 1;
            _triangles[_vertexIndex + 4] = _rowIndex + xMapSize + 1;
            _triangles[_vertexIndex + 5] = _rowIndex + xMapSize + 2;

            _rowIndex++;
            _vertexIndex += 6;
        }

        /// <summary>
        /// The erosion method searches for the script named Erosion attached to this object
        /// and it passes the reference to the erosion variable.
        /// Then it runes the Erode method present in Erosion.cs
        /// Creates the eroded shape for the mesh
        /// and to finish it updates the mesh
        /// </summary>
        public void Erode()
        {
            erosion = FindObjectOfType<Erosion>();
            erosion.Erode(_noiseMap, MapSize, Iterations);
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
}