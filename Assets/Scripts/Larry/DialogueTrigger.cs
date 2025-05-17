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

        if (larryNewPos && larry)
        {
            larry.position = larryNewPos.position;
        }
        GameManager.instance.SetDialogueState(dialogueState);
        gameObject.SetActive(false);
    }
}
