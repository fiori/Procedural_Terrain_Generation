using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MapGenerator : MonoBehaviour{
    [System.Serializable]
    public struct TerrainTypes
    {
        public string Name;
        public float Height;
        public Color Color;
    }

    public enum DrawMode {
        NoiseMap,
        ColorMap
    }

    public DrawMode drawMode;

    public int MapWidht;
    public int MapHeight;
    public float NoiseScale;

    public int Octaves;
    [Range(0,1)]
    public float Persistance;
    public float Lacunarity;

    public int Seed;
    public Vector2 Offset;

    public bool AutoUpdate;

    public TerrainTypes[] Regions;

    public void GenerateMap(){
        //float[,] noiseMap = Noise.
        float[,] noiseMap = Noise.GenerateNoiseMap(MapWidht, MapHeight, Seed, NoiseScale,Octaves,Persistance,Lacunarity, Offset);

        //Loop through the noiseMap, get the height and if the Region has that certain height then print the color
        Color[] colourMap = new Color[MapWidht * MapHeight];
        for (int y = 0; y < MapHeight; y++) {
            for (int x = 0; x < MapWidht; x++) {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < Regions.Length; i++) {
                    if (currentHeight <= Regions[i].Height) {
                        colourMap[y * MapWidht + x] = Regions[i].Color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) 
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        else if (drawMode == DrawMode.ColorMap)
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, MapWidht, MapHeight));
    }


    //This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).
    //Use this function to validate the data of your MonoBehaviours. This can be used to ensure that when you modify data in an editor that the data stays within a certain range.
    //https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnValidate.html

    void OnValidate() {
        if (MapWidht < 1) 
            MapWidht = 1;
        if (MapHeight < 1) 
            MapHeight = 1;
        if (Lacunarity < 1)
            Lacunarity = 1;
        if (Octaves < 0)
            Octaves = 0;
    }

   

}
