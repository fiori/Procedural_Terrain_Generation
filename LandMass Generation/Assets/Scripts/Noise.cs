using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public static class Noise 
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset) {
        float[,] noiseMap = new float[mapWidth,mapHeight];

        //Generates a random number giving the a seed
        System.Random prng = new System.Random(seed);

        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offSetX = prng.Next(-100000, 100000) + offset.x;
            float offSetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offSetX,offSetY);
        }

        if (scale <= 0) {
            scale = 0.00001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        //Calculate the center of the screen, so when using the noise scale it zooms in the middle and not in the corner

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffsets[i].x;
                    float sampleY = (y- halfHeight) / scale * frequency + octaveOffsets[i].y;
                    //Multiplying the sampleY by two and after subtracting one is going to create a negative value.
                    //SampleY = 0 * 2 - 1 = -1 // SampleY = 1 * 2 - 1 = 1;
                    //float perlinValue = Mathf.PerlinNoise(sampleX, sampleY * 2 - 1);
                    float perlinValue = PerlinNoise.Perlin(sampleX, sampleY * 2 - 1);

                    //float perlinValue = PerlinNoise.Perlin(sampleX, sampleY, 0);
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight)
                    maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight)
                    minNoiseHeight = noiseHeight;
                noiseMap[x, y] = noiseHeight;

            }
        }

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                //InverseLerp returns a value 0 - 1 
                //If the noiseMap value is equals to minNoiseHeight it returns 0 if it is equals to maxNoiseHeight it returns 1 and if it is half way it returns 0.5 and so on.
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
