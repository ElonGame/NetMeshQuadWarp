using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class FPSCounter : MonoBehaviour
{
    float deltaTime = 0.0f;

    public Rect  GUIRect = new Rect(0, 0, 512, 32);
    public int   FontSize = 32;
    public Color TextColor = Color.white;

    public int GUIDepth = 100;

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

    void OnGUI()
    {
        GUI.depth = GUIDepth;

        GUIStyle style = new GUIStyle();

        Rect rect = GUIRect;
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = FontSize;
        style.normal.textColor = TextColor;
        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.00} ms ({1:0.00} fps)", msec, fps);
        GUI.Label(rect, text, style);
    }
}