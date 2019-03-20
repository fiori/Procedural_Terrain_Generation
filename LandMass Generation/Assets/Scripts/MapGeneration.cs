using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh
    }

    public DrawMode drawMode;

    public const int mapChunkSize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public float noiseScale;
    public int octaves;

    [Range(0,1)]
    public float persistance;
    public float lacunarity;
    public float TerrainHeight;
    public AnimationCurve heightCurve;
    public int seed;
    public Vector2 offset;
    public bool autoUpdate;
    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale,octaves,persistance,lacunarity, offset);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].Height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].Colour;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawMode == DrawMode.NoiseMap)
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap,mapChunkSize,mapChunkSize));
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainmesh(noiseMap,TerrainHeight, heightCurve,levelOfDetail), TextureGenerator.TextureFromColourMap(colourMap, mapChunkSize, mapChunkSize));
        }
    }

    void OnValidate()
    {

        if (lacunarity < 1)
            lacunarity = 1;
        if (octaves < 0)
            octaves = 0;
    }
    [System.Serializable]
    public struct TerrainType
    {
        public string Name;
        public float Height;
        public Color Colour;
    }


}
