using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PerlinNoise))]
public class PerlinNoiseEditor : Editor
{

    private PerlinNoise instance;

    private void OnEnable()
    {
        instance = (PerlinNoise)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(30);
        if (GUILayout.Button("Generate", GUILayout.Height(30)))
        {
            instance.Generate();
        }

        GUILayout.Space(30);
        if (GUILayout.Button("Save to Disk", GUILayout.Height(30)))
        {
            instance.SaveToDisk();
        }

    }

}
