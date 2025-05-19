using Game;
using UnityEngine;

public class Explodable : MonoBehaviour
{
    [SerializeField] private AudioClip destroySfx;

    public void PlaySound()
    {
        if (destroySfx)
            AudioSource.PlayClipAtPoint(destroySfx, transform.position);

        if (gameObject.CompareTag("SecondLakePartRock"))
        {
            GameManager.instance.SetDialogueState(GameManager.DialogueState.RockExploded);
        }
    }
}
