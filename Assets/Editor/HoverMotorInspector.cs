using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(HoverMotor))]
public class HoverMotorInspector : Editor
{

    void OnSceneGUI()
    {
        HoverMotor motor = target as HoverMotor;
        float hoverHeight = motor.hoverHeight;
        Vector3 origin = motor.transform.position + (Vector3.up * motor.rayCastHeightModifier);
        Handles.color = Color.red;
        //float size = motor.sphereCastRadius * 2f;

        //Handles.SphereHandleCap(0, origin + (Vector3.up * motor.sphereCastRadius), Quaternion.identity, size, EventType.Repaint);
        //Handles.SphereHandleCap(0, origin + (Vector3.up * motor.sphereCastRadius) + (Vector3.down * hoverHeight), Quaternion.identity, size, EventType.Repaint);

        List<Vector3> directions = motor.raycastDirections;
        foreach (Vector3 ray in directions)
        {
            float dot = (Vector3.Dot(Vector3.up, ray.normalized) + 1) * 0.5f;

            float rayLength = hoverHeight + (hoverHeight * dot) * motor.rayCastHorizontalLengthModifier;
            Handles.DrawLine(origin, origin + (ray.normalized * rayLength));
        }
    }

    //void OnDrawGizmosSelected()
    //{
    //    HoverMotor motor = target as HoverMotor;
    //    float hoverHeight = motor.hoverHeight;
    //    Vector3 origin = motor.transform.position;
    //    Handles.colour = Color.red;
    //    float size = motor.sphereCastRadius;

    //    //    //Handles.SphereHandleCap(0, origin + (Vector3.up * motor.sphereCastRadius), Quaternion.identity, size, EventType.Repaint);
    //    //    Handles.SphereHandleCap(0, origin + (Vector3.up * motor.sphereCastRadius) + (Vector3.down * hoverHeight), Quaternion.identity, size, EventType.Repaint);

    //    Gizmos.colour = Color.red;
    //    Gizmos.DrawWireSphere(origin + (Vector3.up * size) + (Vector3.down * hoverHeight), size);
    //}
}
