using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterGeneration : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void CreateWater()
    {
        int lod = 17;
        int verticesperLine = (MeshGeneration.instance.mapSize / lod);
        Vector3[] verticies = new Vector3[(verticesperLine + 1) * (verticesperLine + 1)];
        int verticeIndex = 0;

        for (int z = 0; z < MeshGeneration.instance.mapSize; z+=lod)
        {
            for (int x = 0; x < MeshGeneration.instance.mapSize; x+=lod)
            {
                int randomNumber = Random.Range(20, 60);
                verticies[verticeIndex] = new Vector3(x, randomNumber, z);
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Material sphereM = sphere.GetComponent<Renderer>().sharedMaterial;
                sphereM.color = Color.blue;
                sphere.AddComponent<Rigidbody>();
                sphere.transform.position = verticies[verticeIndex];
                verticeIndex++;
            }
          
        }



    }
}
