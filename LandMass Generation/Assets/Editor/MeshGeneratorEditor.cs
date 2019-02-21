using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(newMeshGenerator))]
public class MeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        newMeshGenerator mesh = (newMeshGenerator) target;

        if (GUILayout.Button("RenderMesh"))
        {
            mesh.CreateMesh();
        }
    }
}
