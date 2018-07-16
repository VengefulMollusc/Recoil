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
        Rigidbody rb = motor.GetComponent<Rigidbody>();

        //Handles.SphereHandleCap(0, origin + (Vector3.up * motor.sphereCastRadius), Quaternion.identity, size, EventType.Repaint);
        //Handles.SphereHandleCap(0, origin + (Vector3.up * motor.sphereCastRadius) + (Vector3.down * hoverHeight), Quaternion.identity, size, EventType.Repaint);

        List<Vector3> directions = motor.raycastDirections;
        foreach (Vector3 ray in directions)
        {
            // extend raycast length depending on angle from vertical
            float verticalDot = (Vector3.Dot(Vector3.up, ray.normalized) + 1) * 0.5f;
            float verticalSpread = verticalDot * hoverHeight * motor.rayCastHorizontalLengthModifier;

            // extend raycast length depending on angle to movement direction
            //Vector3 movementVector = new Vector3(0f, 0f, 1f);
            Vector3 movementVector = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            float movementDot = Vector3.Dot(movementVector.normalized, ray.normalized);
            movementDot = (movementDot - 0.75f) * 4f;
            if (movementDot < 0f)
                movementDot = 0f;
            float movementSpread = movementDot * hoverHeight * motor.rayCastHorizontalLengthModifier;

            float rayLength = hoverHeight + verticalSpread + movementSpread;
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
