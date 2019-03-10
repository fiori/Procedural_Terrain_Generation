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
                mesh.SetMeshData();
                mesh.CreateShape();
                mesh.UpdateMesh();
            }
        }

        if (GUILayout.Button("Generate"))
        {
            mesh.SetMeshData();
            mesh.CreateShape();
            mesh.UpdateMesh();
        }
    }
}
