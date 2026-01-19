using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    MapGenerator mapGenerator;

    void OnEnable()
    {
        mapGenerator = (MapGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();


        if (GUILayout.Button("Generate Map"))
        {
            mapGenerator.DrawMapInEditor();
        }

        if (GUI.changed && mapGenerator.autoUpdate)
        {
            mapGenerator.DrawMapInEditor();
        }
        
    }
}