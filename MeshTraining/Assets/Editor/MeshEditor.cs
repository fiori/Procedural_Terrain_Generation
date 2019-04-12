using Assets.Scripts;
using UnityEditor;
using UnityEngine;

namespace Assets.Editor
{
    [CustomEditor(typeof(MeshGeneration))]
    public class MeshEditor : UnityEditor.Editor
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
            if (GUILayout.Button("Erosion"))
            {
                //WaterGeneration.CreateWater();
                mesh.Erode();

            }
        }
    }
}
