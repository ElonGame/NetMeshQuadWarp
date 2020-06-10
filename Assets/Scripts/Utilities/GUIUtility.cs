using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Mathematics;
using static Unity.Mathematics.math;

namespace NetValley
{
    public static class GUIUtility
    {
        static public void DrawString(string text, Vector3 worldPos, Color? colour = null)
        {
#if UNITY_EDITOR
            UnityEditor.Handles.BeginGUI();

            var restoreColor = GUI.color;

            if (colour.HasValue) GUI.color = colour.Value;
            var view = UnityEditor.SceneView.currentDrawingSceneView;
            Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);

            if (screenPos.y < 0 || screenPos.y > Screen.height || screenPos.x < 0 || screenPos.x > Screen.width || screenPos.z < 0)
            {
                GUI.color = restoreColor;
                UnityEditor.Handles.EndGUI();
                return;
            }

            Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
            GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
            GUI.color = restoreColor;
            UnityEditor.Handles.EndGUI();
#endif
        }

        static public void RectXZ(float3 center, float2 size, Color? colour = null)
        {
            var restoreColor = Gizmos.color;

            if (colour.HasValue) Gizmos.color = colour.Value;

            Gizmos.DrawLine(float3(center.x - size.x * 0.5f, center.y, center.z - size.y * 0.5f), float3(center.x + size.x * 0.5f, center.y, center.z - size.y * 0.5f));
            Gizmos.DrawLine(float3(center.x + size.x * 0.5f, center.y, center.z - size.y * 0.5f), float3(center.x + size.x * 0.5f, center.y, center.z + size.y * 0.5f));
            Gizmos.DrawLine(float3(center.x + size.x * 0.5f, center.y, center.z + size.y * 0.5f), float3(center.x - size.x * 0.5f, center.y, center.z + size.y * 0.5f));
            Gizmos.DrawLine(float3(center.x - size.x * 0.5f, center.y, center.z + size.y * 0.5f), float3(center.x - size.x * 0.5f, center.y, center.z - size.y * 0.5f));


            Gizmos.color = restoreColor;
        }

        static public void DrawWireRect(float3 LB, float3 LT, float3 RT, float3 RB, Color? colour = null)
        {
            var restoreColor = Gizmos.color;

            if (colour.HasValue) Gizmos.color = colour.Value;
            {
                Gizmos.DrawLine(LB, LT);
                Gizmos.DrawLine(LT, RT);
                Gizmos.DrawLine(RT, RB);
                Gizmos.DrawLine(RB, LB);
            }

            Gizmos.color = restoreColor;

        }
    }
}