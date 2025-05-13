using UnityEngine;
using TMPro;

/// <summary>
/// Code généré pour afficher la console sur un canvas pour aider au debug en étant en jeu
/// TODO : A supprimer plus tard car utilisé que pour des tests
/// </summary>
public class UIConsoleLogger : MonoBehaviour
{
    [Header("Drag your Text or TMP_Text here")]
    [SerializeField]
    private TMP_Text output;       // Or: public Text _output;

    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Directly set the UI text to the latest log entry
        output.text = logString;
    }

}