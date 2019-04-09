using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Erosion : MonoBehaviour
{
    public float initialSpeed = 1;
    public float initialWaterVolume = 1;
    public int ParticleLifeTime = 30;
    public int erosionRadius = 3;
    [Range(0, 1)] public float intertia = 0.2f;
    System.Random rng;

    //Erosion brush for every node
    //Jagged arrays
    private int[][] erosionBrushIndices;
    private float[][] erosionBrushWeights;

    private int currentErosionRadius;
    private int currentMapSize;

    void Initialize(int mapSize)
    {
        if (rng == null)
        {
            rng = new System.Random(15);
        }

        if (erosionBrushIndices == null || currentMapSize != mapSize || currentErosionRadius != erosionRadius)
        {
            InitializeBrushIndices(mapSize,erosionRadius);
            currentErosionRadius = erosionRadius;
            currentMapSize = mapSize;
        }
    }

    public void Erode(float[] map, int mapSize, int numIterations = 1)
    {
        Initialize(mapSize);

        for (int iteration = 0; iteration < numIterations; iteration++)
        {
            //Create water droplet at random point on the map
            Vector2 ParticlePosition = new Vector2(rng.Next(0, mapSize - 1), rng.Next(0, mapSize - 1));
            Vector2 ParticleDirection = new Vector2(0, 0);
            float ParticleVelocity = initialSpeed;
            float WaterAmount = initialWaterVolume;
            float Sediment = 0;
            for (int i = 0; i < ParticleLifeTime; i++)
            {
                int nodeX = (int) ParticlePosition.x;
                int nodeY = (int) ParticlePosition.y;
                int dropletIndex = nodeY * mapSize + nodeX;

                float cellOffsetX = ParticlePosition.x - nodeX;
                float cellOffsetY = ParticlePosition.y - nodeY;

                //Calculate height of the four nodes of the particle
                HeightAndGradient heightAndGradient = CalculateHeightAndGradient(map, mapSize, ParticlePosition.x, ParticlePosition.y);


                //Update the droplets position (move 1 unit regardless of speed so as not to skip over sections of the map)
                ParticleDirection = new Vector2(ParticleDirection.x * intertia - heightAndGradient.gradientX * (1-intertia), 
                    ParticleDirection.y * intertia - heightAndGradient.gradientY * (1 - intertia));
                
                //Normalize direction
                ParticlePosition += ParticleDirection.normalized;

                //Find the droplets new height and calculate the deltaheight

                //Calculate the droplets sediment capacity (higher when moving fast down a slope and contains lots of water)

                //If carrying more sediment than capacity, or if flowing up a slope: 
                //Depois a fraction of the sediment to the surrounding nodes (with bilinear interpolation)

                //Otherwise
                //Erode a fraction of the droplets reamining capacity from the soil, distributed over the radius of the droplet
                //Note: don't erode more than deltaheight to avoid digging holes behind the droplet and creating spikes
                for (int brushPointInd = 0; brushPointInd < erosionBrushIndices[dropletIndex].Length; brushPointInd++)
                {
                    int nodeIndex = erosionBrushIndices[dropletIndex][brushPointInd];
                    float weighedErodeAmount = 2 * erosionBrushWeights[dropletIndex][brushPointInd];
                    float deltaSediment = (map[nodeIndex] < 10) ? map[nodeIndex] : weighedErodeAmount;
                    map[nodeIndex] -= deltaSediment;
                    Sediment +=deltaSediment;
                }

                //Update droplets speed based on deltaheight
                //Evaporate a fraction of the droplets water
            }
        }

    }

    HeightAndGradient CalculateHeightAndGradient(float[] nodes, int mapSize, float posX, float posY)
    {
        int coordX = (int) posX;
        int coordY = (int) posY;

        float x = posX - coordX;
        float y = posY - coordY;

        int dropletIndex = coordY * mapSize + coordX;

        float height00 = nodes[dropletIndex];
        float height01 = nodes[dropletIndex + 1];
        float height02 = nodes[dropletIndex + mapSize];
        float height03 = nodes[dropletIndex + mapSize + 1];
        float gradientX = (height01 - height00) * (1 - y) + (height03 - height02) * y;
        float gradientY = (height02 - height00) * (1 - x) + (height03 - height01) * x;
        //Calculate droplet's height and the direction of flow with bilinear interpolation of surround heights
        float height = height00 * (1 - x) * (1 - y) + 
                       height01 * x * (1 - y) +
                       height02 * (1 - x) * y +
                       height03 * x * y;
        return new HeightAndGradient() { height = height, gradientX = gradientX, gradientY = gradientY };
    }

    void InitializeBrushIndices(int mapSize, int radius)
    {
        erosionBrushIndices = new int[mapSize * mapSize][];
        erosionBrushWeights = new float[mapSize * mapSize][];

        int[] xOffsets = new int[radius * radius * 4];
        int[] yOffsets = new int[radius * radius * 4];
        float[] weights = new float[radius * radius * 4];
        float weightSum = 0;
        int addIndex = 0;


        for (int i = 0; i < erosionBrushIndices.GetLength(0); i++)
        {
            int centreX = i % mapSize;
            int centreY = i / mapSize;

            if (centreY <= radius || centreY >= mapSize - radius || centreX <= radius + 1 ||
                centreX >= mapSize - radius)
            {
                weightSum = 0;
                addIndex = 0;
                for (int y = -radius; y <= radius; y++)
                {
                    for (int x = -radius; x <= radius; x++)
                    {
                        float sqrDst = x * x + y * y;
                        if (sqrDst < radius * radius)
                        {
                            int coordX = centreX + x;
                            int coordY = centreY + y;

                            if (coordX >= 0 && coordX < mapSize && coordY >= 0 && coordY < mapSize)
                            {
                                float weight = 1 - Mathf.Sqrt(sqrDst) / radius;
                                weightSum += weight;
                                weights[addIndex] = weight;
                                xOffsets[addIndex] = x;
                                yOffsets[addIndex] = y;
                                addIndex++;
                            }
                        }
                    }
                    
                }
            }

            int numEntries = addIndex;
            erosionBrushIndices[i] = new int[numEntries];
            erosionBrushWeights[i] = new float[numEntries];

            for (int j = 0; j < numEntries; j++)
            {
                erosionBrushIndices[i][j] = (yOffsets[j] + centreY) * mapSize + xOffsets[j] + centreX;
                erosionBrushWeights[i][j] = weights[j] / weightSum;
            }
        }
    }
    struct HeightAndGradient
    {
        public float height;
        public float gradientX;
        public float gradientY;
    }
}
