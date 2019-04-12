using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
    internal struct Gradient
    {
        public float Height;
        public float X;
        public float Y;
    }


    public class Erosion : MonoBehaviour
    {
        [Range(0,1)]
        public float Radius = 0.1f;

        [Range(0, 1)]
        public float ErosionSpeed = .3f;

        [Range(0, 1)]
        public float DepositionSpeed = .3f;

        public int WaterDropLifeTime = 30;

        public float StartingWaterAmount = 1;

        public float StartingSpeedOfTheWaterFlow = 1;

        //Private Vars
        private const float MaxSedimentAmount = 4; // Multiplier for how much sediment a droplet can carry
        private const float MinSedimentAmount = .01f; // Used to prevent carry capacity getting too close to zero on flatter terrain

        private System.Random _rng;
        //seed that is used in the initialize method and passed to the current seed, don't know if it is needed
        private int _seed;

        public void Erode(float[] map, int mapSize, int numIterations = 1, bool resetSeed = false)
        {
            //Initialize(mapSize, resetSeed);
            if (_rng == null)
            {
                _seed = Random.Range(0, 100);
                _rng = new System.Random(_seed);
            }


            for (int iteration = 0; iteration < numIterations; iteration++)
            {
                // Create water droplet at random point on map
                float posX = _rng.Next(0, mapSize - 1);
                float posY = _rng.Next(0, mapSize - 1);
                float dirX = 0;
                float dirY = 0;
                float waterVelocity = StartingSpeedOfTheWaterFlow;
                float waterAmount = StartingWaterAmount;
                float sediment = 0;

                for (int lifetime = 0; lifetime < WaterDropLifeTime; lifetime++)
                {
                    int nodeX = (int)posX;
                    int nodeY = (int)posY;
                    int dropletIndex = nodeY * mapSize + nodeX;
                    // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
                    float cellOffsetX = posX - nodeX;
                    float cellOffsetY = posY - nodeY;

                    // Calculate droplet's height and direction of flow with bilinear interpolation of surrounding heights
                    Gradient gradient = CalculateGradient(map, mapSize, posX, posY);

                    // Update the droplet's direction and position (move position 1 unit regardless of speed)
                    dirX = (dirX - gradient.X);
                    dirY = (dirY - gradient.Y);
                    // Normalize direction
                    float len = Mathf.Sqrt(dirX * dirX + dirY * dirY);
                    if (len != 0)
                    {
                        dirX /= len;
                        dirY /= len;
                    }
                
                    posX += dirX;
                    posY += dirY;

                    // Stop simulating droplet if it's not moving or has flowed over edge of map, this code if extremly important without it, the index goes outside the bounds of the array.
                    // Because it keeps calculating the gradient of the drop to the infinity. That is why that if the drop is outside the map or the direction is 0 this means it stopped moving it breaks.
                    if ((dirX == 0 && dirY == 0) || posX < 0 || posX >= mapSize - 1 || posY < 0 || posY >= mapSize - 1)
                    {
                        break;
                    }

                    // Find the droplet's new height and calculate the deltaHeight
                    float newHeight = CalculateGradient(map, mapSize, posX, posY).Height;
                    float deltaHeight = newHeight - gradient.Height;

                    // Calculate the droplet's sediment capacity (higher when moving fast down a slope and contains lots of water)
                    float sedimentCapacity = Mathf.Max(-deltaHeight * waterVelocity * waterAmount * MaxSedimentAmount, MinSedimentAmount);

                    ///////////////////////////////////////////////////////
                    /// Where the magic happens

                    // If carrying more sediment than capacity, or if flowing uphill:
                    if (sediment > sedimentCapacity || deltaHeight > 0)
                    {
                        // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
                        float amountToDeposit = (deltaHeight > 0) ? Mathf.Min(deltaHeight, sediment) : (sediment - sedimentCapacity) * DepositionSpeed;
                        sediment -= amountToDeposit;

                        // Add the sediment to the four nodes of the current cell using bilinear interpolation
                        // Deposition is not distributed over a radius (like erosion) so that it can fill small pits
                        map[dropletIndex] += amountToDeposit * (1 - cellOffsetX) * (1 - cellOffsetY);
                        map[dropletIndex + 1] += amountToDeposit * cellOffsetX * (1 - cellOffsetY);
                        map[dropletIndex + mapSize] += amountToDeposit * (1 - cellOffsetX) * cellOffsetY;
                        map[dropletIndex + mapSize + 1] += amountToDeposit * cellOffsetX * cellOffsetY;

                    }
                    else
                    {
                        // Erode a fraction of the droplet's current carry capacity.
                        // Clamp the erosion to the change in height so that it doesn't dig a hole in the terrain behind the droplet
                        float amountToErode = Mathf.Min((sedimentCapacity - sediment) * ErosionSpeed, -deltaHeight);

                        float weighedErodeAmount = amountToErode * Random.Range(0.01f, Radius);
                        float deltaSediment = (map[dropletIndex] < weighedErodeAmount) ? map[dropletIndex] : weighedErodeAmount;
                        map[dropletIndex] -= deltaSediment;
                        sediment += deltaSediment;
                    }
                }
            }
        }

        private Gradient CalculateGradient(float[] nodes, int mapSize, float posX, float posY)
        {
            int coordX = (int)posX;
            int coordY = (int)posY;

            // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
            float x = posX - coordX;
            float y = posY - coordY;

            // Calculate heights of the four nodes of the droplet's cell
            int nodeIndexNW = coordY * mapSize + coordX;
            ///////////////////////////////////////////////////
            //NODE(x)(y)
            //    (0)(0)
            //    (1)(0)
            //    (0)(1)
            //    (1)(1)
            float node00 = nodes[nodeIndexNW];
            float node10 = nodes[nodeIndexNW + 1];
            float node01 = nodes[nodeIndexNW + mapSize];
            float node11 = nodes[nodeIndexNW + mapSize + 1];

            // Calculate droplet's direction of flow with bilinear interpolation of height difference along the edges
            float slopeX = (node10 - node00) * (1 - y) + (node11 - node01) * y;
            float slopeY = (node01 - node00) * (1 - x) + (node11 - node10) * x;

            // Calculate height with bilinear interpolation of the heights of the nodes of the cell
            float height = node00 * (1 - x) * (1 - y) + node10 * x * (1 - y) + node01 * (1 - x) * y + node11 * x * y;

            return new Gradient() { Height = height, X = slopeX, Y = slopeY };
        }
    }
}