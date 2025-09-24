using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class OnScreenLoger : MonoBehaviour
{
    public Text uiText; // arrastra un UI Text
    public int maxLines = 20;
    Queue<string> q = new Queue<string>();

    void Awake()
    {
        Application.logMessageReceived += HandleLog;
    }
    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }
    void HandleLog(string logString, string stackTrace, LogType type)
    {
        q.Enqueue($"[{type}] {logString}");
        if (q.Count > maxLines) q.Dequeue();
        if (uiText) uiText.text = string.Join("\n", q.ToArray());
        // opcional: log a archivo en dispositivo
        try { File.AppendAllText(Path.Combine(Application.persistentDataPath, "device_log.txt"), $"[{type}] {logString}\n"); }
        catch { }
    }
}
