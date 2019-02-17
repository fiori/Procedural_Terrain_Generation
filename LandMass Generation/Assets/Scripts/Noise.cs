using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth,mapHeight];

        System.Random rng = new System.Random(seed);
        Vector2[] octaveOffesetVector2s = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = rng.Next(-100000, 100000) + offset.x;
            float offsetY = rng.Next(-100000, 100000) + offset.y;
            octaveOffesetVector2s[i] = new Vector2(offsetX,offsetY);
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxNoiseheight = float.MinValue;
        float minNoiseheight = float.MaxValue;

        float halfWidht = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;
                for (int i = 0; i < octaves; i++)
                {
                    float valueX = (x - halfWidht) / scale * frequency + octaveOffesetVector2s[i].x;
                    float valueY = (y - halfHeight) / scale * frequency + octaveOffesetVector2s[i].y;
                    float perlinValue = PerlinNoise.Perlin(valueX, valueY, 0) ;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseheight)
                    maxNoiseheight = noiseHeight;
                else if (noiseHeight < minNoiseheight)
                    minNoiseheight = noiseHeight;
                noiseMap[x, y] = noiseHeight;
            }
        }
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseheight, maxNoiseheight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
