using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MachineGun))]
public class MachineGunInspector : Editor {

    void OnSceneGUI()
    {
        MachineGun mg = target as MachineGun;
        Transform mgTransform = mg.transform;
        Vector3 origin = mg.transform.position;
        Handles.color = Color.red;

        List<Vector3> firingPoints = mg.firingPoints;
        for (int i = 0; i < firingPoints.Count; i++)
        {
            Vector3 point = origin + firingPoints[i];
            Handles.DrawLine(origin, point);
            if (mg.editFiringPoints)
            {
                EditorGUI.BeginChangeCheck();
                point = Handles.DoPositionHandle(point, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(mg, "Move Point");
                    EditorUtility.SetDirty(mg);
                    firingPoints[i] = mgTransform.InverseTransformPoint(point);
                }
            }
        }
    }
}
