using UnityEngine;

namespace Assets.Scripts
{
    public static class TextureGeneration 
    {
        /// <summary>
        /// Creates a colour map for the mesh
        /// </summary>
        /// <param name="mapSize">The total size of the Mesh</param>
        /// <param name="terrains">Struct defined in MeshGeneration</param>
        /// <param name="noiseMap">The noise map created inside the MapGeneration.GenerateNoiseMap()</param>
        /// <returns>Returns a Texture2D to be applied in a mesh</returns>
        public static Texture2D ColourMap(int mapSize, TerrainType[] terrains, float[] noiseMap)
        {
            Color[] colourMap = new Color[mapSize * mapSize];

            for (int y = 0; y < mapSize; y++)
            {
                for (int x = 0; x < mapSize; x++)
                {
                    float currentNoiseMapHeight = noiseMap[y * mapSize + x];

                    for (int i = 0; i < terrains.Length; i++)
                    {
                        if (currentNoiseMapHeight <= terrains[i].height)
                        {
                            colourMap[y * mapSize + x] = terrains[i].colour;
                            break;
                        }
                    }
                }
            }

            Texture2D texture = new Texture2D(mapSize, mapSize);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }
    }
}
