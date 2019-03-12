using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MapGeneration 
{
    //Lacunarity = control the increase in frequency of octaves
    //Persistance = control the increase in amplitude of octaves
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float noiseScale, float lacunarity, float persistance, int octaves)
    {
        float[,] map = new float[mapWidth + 1, mapHeight + 1];

        if (noiseScale <= 0)
        {
            noiseScale = 0.00001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        for (int y = 0; y <= mapHeight; y++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                //Controls the height of the noise map
                float amplitude = 1;
                //Controls the width of the noise map
                float frequency = 1;
                float noiseHeight = 0;
                for (int o = 0; o < octaves; o++)
                {
                    float valueX = x / noiseScale * frequency;
                    float valueY = y / noiseScale * frequency;

                    float perlinValue = PerlinNoise.Perlin(valueX, valueY, 0);
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;

                }
                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;
                map[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y <= mapHeight; y++)
        {
            for (int x = 0; x <= mapWidth; x++)
            {
                //Making sure that the noiseMap is only between [0,1] values
                map[x, y] = (map[x, y] - minNoiseHeight) / (maxNoiseHeight - minNoiseHeight);
            }
        }

        return map;
    }
}
