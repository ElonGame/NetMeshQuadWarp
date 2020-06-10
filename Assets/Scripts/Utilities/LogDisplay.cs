using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// GUI に ログメッセージ (Debug.Log("")) を表示する
/// </summary>
[ExecuteInEditMode]
public class LogDisplay : MonoBehaviour
{
    [SerializeField]
    int _maxLogCount = 20;

    [SerializeField]
    Rect _area = new Rect(220, 0, 400, 400);

    public int GUIDepth = 100;
    
    Queue<string> _logMessages = new Queue<string>();

    System.Text.StringBuilder _stringBuilder = new System.Text.StringBuilder();

    private void Awake()
    {
        Application.logMessageReceived += LogReceived;
    }

    private void OnDestroy()
    {
        Application.logMessageReceived -= LogReceived;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= LogReceived;
    }

    void LogReceived(string text, string stackTrace, LogType type)
    {
        _logMessages.Enqueue(text);
        while (_logMessages.Count > _maxLogCount)
        {
            _logMessages.Dequeue();
        }
    }

    void OnGUI()
    {
        _stringBuilder.Length = 0;
        foreach (string s in _logMessages)
        {
            _stringBuilder.Append(s).Append(System.Environment.NewLine);
        }

        GUI.depth = GUIDepth;
        GUI.Label(_area, _stringBuilder.ToString());
    }
}