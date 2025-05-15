using System;
using Game;
using UnityEngine;

public class DialogueTrrigger : MonoBehaviour
{
    public GameManager.DialogueState dialogueState;

    private void OnTriggerEnter(Collider other)
    {
        GameManager.instance.SetDialogueState(dialogueState);
    }
}
