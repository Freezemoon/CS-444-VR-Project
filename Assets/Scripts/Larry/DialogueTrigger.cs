using System;
using Game;
using UnityEngine;

public class DialogueTrrigger : MonoBehaviour
{
    public GameManager.DialogueState dialogueState;
    public Transform larryNewPos;
    public Transform larry;
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        switch (dialogueState)
        {
            case GameManager.DialogueState.RockDynamite:
                if (GameManager.instance.State.DynamiteAmount <= 0)
                {
                    return;
                }
                break;
        }
        
        if (GameManager.instance.SetDialogueState(dialogueState))
            ValidateDialogue();
    }

    public void ValidateDialogue()
    {
        if (larryNewPos && larry)
        {
            larry.position = larryNewPos.position;
        }
        gameObject.SetActive(false);
    }
}
