using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapPreview))]
public class MapPreviewEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapPreview mapPreview = target as MapPreview;

        if (DrawDefaultInspector())
        {
            if (mapPreview.autoUpdate)
                mapPreview.DrawMapInEditor();
        }

        if (GUILayout.Button("Generate"))
        {
            mapPreview.DrawMapInEditor();
        }
    }
}
