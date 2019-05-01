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
        public float Radius = 0.1f; ///Defines the diameter of each particle

        [Range(0, 1)]
        public float ErosionSpeed = .3f; ///used to increase the erosion speed

        [Range(0, 1)]
        public float DepositionSpeed = .3f;///Used to increase the deposition speed 

        public int WaterDropLifeTime = 30; ///LifeSpawn of each particle

        public float StartingWaterAmount = 1; ///The initializer for the amount of water

        public float StartingSpeedOfTheWaterFlow = 1; ///The initial speed of the water flow

        //Private Vars
        /// Multiplier for how much sediment a droplet can carry
        private const float MaxSedimentAmount = 4;
        /// Used to prevent carry capacity getting too close to zero on flatter terrain
        private const float MinSedimentAmount = .01f; 

        private System.Random _rng;
        ///seed that is used in the initialize method and passed to the current seed, don't know if it is needed
        private int _seed;

        /// <summary>
        /// Method using for creating the erosion on the mesh
        /// </summary>
        /// <param name="map">The heightmap generated in MapGeneration with the Perlin Noise</param>
        /// <param name="mapSize">The size generated from the Mesh</param>
        /// <param name="numIterations">Generated Particle Quantity</param>
        public void Erode(float[] map, int mapSize, int numIterations = 1)
        {
            if (_rng == null)
            {
                _seed = Random.Range(0, 100);
                _rng = new System.Random(_seed);
            }


            for (int iteration = 0; iteration < numIterations; iteration++)
            {
                // Create water drop at a random location on the map
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
                    // Calculate droplet's offset inside the cell (0,0) = at node00, (1,1) = at node11
                    float cellOffsetX = posX - nodeX;
                    float cellOffsetY = posY - nodeY;

                    //Calculate the gradient for the droplet
                    Gradient gradient = CalculateGradient(map, mapSize, posX, posY);

                    // Update the droplet's direction
                    dirX -= gradient.X;
                    dirY -= gradient.Y;

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

                    // Find the droplet's new height
                    float newHeight = CalculateGradient(map, mapSize, posX, posY).Height;
                    float deltaHeight = newHeight - gradient.Height;

                    // Calculate the droplet's sediment capacity (higher when moving fast down a slope and contains lots of water)
                    float sedimentCapacity = Mathf.Max(-deltaHeight * waterVelocity * waterAmount * MaxSedimentAmount, MinSedimentAmount);

                    ///////////////////////////////////////////////////////
                    // Where the magic happens

                    // If carrying more sediment than capacity, or if flowing uphill:
                    if (sediment > sedimentCapacity || deltaHeight > 0)
                    {
                        // If moving uphill (deltaHeight > 0) try fill up to the current height, otherwise deposit a fraction of the excess sediment
                        float amountToDeposit;
                        if (deltaHeight > 0)
                            amountToDeposit = Mathf.Min(deltaHeight, sediment);
                        else
                            amountToDeposit = (sediment - sedimentCapacity) * DepositionSpeed;

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
                        // Clamp the amount to erode
                        float amountToErode = Mathf.Min((sedimentCapacity - sediment) * ErosionSpeed, -deltaHeight);

                        float weighedErodeAmount = amountToErode * Random.Range(0.01f, Radius);
                        float deltaSediment = (map[dropletIndex] < weighedErodeAmount) ? map[dropletIndex] : weighedErodeAmount;
                        map[dropletIndex] -= deltaSediment;
                        sediment += deltaSediment;
                    }
                }
            }
        }

        /// <summary>
        /// Method Responsible to calculate the slope or gradient of the terrain, how steep the terrain is and the best path
        /// for the water particle go to.
        /// </summary>
        /// <param name="nodes">This parameter is the same as map[], is the heightmap generated in MapGeneration with
        /// the Perlin noise.</param>
        /// <param name="mapSize">The size of the map, this is defined on MeshGeneration when creating the mesh</param>
        /// <param name="posX">The position X passed when creating the particle in the erode method</param>
        /// <param name="posY">The position Y passed when creating the particle in the erode method</param>
        /// <returns>Returns the Gradient Struct with the final vector2 position and height for where the water particle
        /// should go.</returns>
        private Gradient CalculateGradient(float[] nodes, int mapSize, float posX, float posY)
        {
            int coordX = (int)posX;
            int coordY = (int)posY;

            // Calculate droplet's offset inside the cell (0,0) = at NW node, (1,1) = at SE node
            float x = posX - coordX;
            float y = posY - coordY;

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