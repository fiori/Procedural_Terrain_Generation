namespace Assets.Scripts
{
    public static class MapGeneration 
    {
        //Lacunarity = control the increase in frequency of octaves
        //Persistance = control the increase in amplitude of octaves
        /// <summary>
        /// Generates a noiseMap through the PerlinNoise class
        /// </summary>
        /// <param name="mapSize">The pretended size of the map</param>
        /// <param name="noiseScale">This parameter is responsible for the scale of the NoiseMap, if we increase this value
        /// the noiseMap is zoomed in. In other words is the distance that we view the noiseMap</param>
        /// <param name="lacunarity">Controls the increase in frequency of octaves, the width of the wave</param>
        /// <param name="persistance">Controls the increase in amplitude of octaves, the height of the wave</param>
        /// <param name="octaves">Octave is the number of level of detail for the perlin noise</param>
        /// <returns>Returns a float[] with the NoiseMap</returns>
        public static float[] GenerateNoiseMap(int mapSize, float noiseScale, float lacunarity, float persistance, int octaves)
        {
            float[] map = new float[(mapSize + 1) * (mapSize + 1)];

            if (noiseScale <= 0)
            {
                noiseScale = 0.00001f;
            }

            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;

            for (int y = 0; y <= mapSize; y++)
            {
                for (int x = 0; x <= mapSize; x++)
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
                    map[y * mapSize + x] = noiseHeight;
                }
            }

            for (int y = 0; y <= mapSize; y++)
            {
                for (int x = 0; x <= mapSize; x++)
                {
                    //Making sure that the noiseMap is only between [0,1] values
                    map[y * mapSize + x] = (map[y * mapSize + x] - minNoiseHeight) / (maxNoiseHeight - minNoiseHeight);
                }
            }

            return map;
        }
    }
}
