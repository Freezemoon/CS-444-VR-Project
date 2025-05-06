using UnityEngine;
using TMPro;
using UnityEngine.Serialization; // Remove if you use UnityEngine.UI.Text

/// <summary>
/// Code généré pour afficfher la console sur un canvas pour aider au debug en étant en jeu
/// </summary>
public class UIConsoleLogger : MonoBehaviour
{
    [FormerlySerializedAs("_output")]
    [Header("Drag your Text or TMP_Text here")]
    [SerializeField]
    private TMP_Text output;       // Or: public Text _output;

    readonly System.Collections.Generic.List<string> _logs = new();

    void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Directly set the UI text to the latest log entry
        output.text = logString;
    }

}