using UnityEngine;

namespace Assets.Scripts
{
    public static class TextureGeneration 
    {
        public static Texture2D ColourMap(int width, int height, TerrainType[] terrains, float[] noiseMap)
        {
            Color[] colourMap = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //TODO: Need to pass the map size instead of 255.
                    float currentNoiseMapHeight = noiseMap[y * 255 + x];

                    for (int i = 0; i < terrains.Length; i++)
                    {
                        if (currentNoiseMapHeight <= terrains[i].height)
                        {
                            colourMap[y * width + x] = terrains[i].colour;
                            break;
                        }    
                    }
                }
            }

            Texture2D texture = new Texture2D(width, height);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }
    }
}
