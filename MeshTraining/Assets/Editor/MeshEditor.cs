using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MeshGeneration))]
public class MeshEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MeshGeneration mesh = (MeshGeneration) target;

        if (DrawDefaultInspector())
        {
            if (mesh.AutoUpdate)
            {
                MeshGeneration.instance = mesh;
                mesh.SetMeshData();
                mesh.CreateShape();
                mesh.UpdateMesh();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            MeshGeneration.instance = mesh;
            //WaterGeneration.CreateWater();
            mesh.SetMeshData();
            mesh.CreateShape();
            mesh.UpdateMesh();
        }
        if (GUILayout.Button("ErodeMyDick"))
        {
            //WaterGeneration.CreateWater();
            mesh.Erode();

        }
    }
}
