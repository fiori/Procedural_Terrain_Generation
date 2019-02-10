using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MapGeneration))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI() {
        MapGeneration mapGen = (MapGeneration)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateMap();
            }
        }
        

        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateMap();
        }
        
    }
}
