using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HeightMapSettings))]
public class HeightMapSettingsEditor : UpdatableDataEditor {

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        HeightMapSettings heightMapSettings = target as HeightMapSettings;

        if (GUILayout.Button("Randomise Noise"))
        {
            heightMapSettings.RandomiseNoise();
            heightMapSettings.NotifyOfUpdatedValues();
            EditorUtility.SetDirty(target);
        }
    }
}
