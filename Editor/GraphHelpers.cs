using System;
using UnityEngine;
using UnityEditor;

public static class GraphHelpers
{
    public enum ConnectionStyle {
        Line,
        Bezier
    }
    public static void DrawLine(Vector2 outPoint, Vector2 inPoint, ConnectionStyle style) {
        if (style == ConnectionStyle.Bezier) {
            Handles.DrawBezier(
                outPoint,
                inPoint,
                outPoint + Vector2.left * 50f,
                inPoint - Vector2.left * 50f,
                Color.yellow,
                null,
                3f
            );
        } else {
            Handles.color = Color.yellow;
            float height = (inPoint - outPoint).y;
            Vector2 controlPoint1 = outPoint + new Vector2(0, height * 0.5f);
            Vector2 controlPoint2 = inPoint - new Vector2(0, height * 0.5f);
            Handles.DrawAAPolyLine(3f, outPoint, controlPoint1);
            Handles.DrawAAPolyLine(3f, controlPoint1, controlPoint2);
            Handles.DrawAAPolyLine(3f, controlPoint2, inPoint);
            Handles.color = Color.white;            
        }
        GUI.changed = true;
    }
}
