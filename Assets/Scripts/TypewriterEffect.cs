using System.Collections;
using Game;
using TMPro;
using UnityEngine;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text textComponent;
    public float typingSpeed = 0.05f;

    [Header("Bouncing Target")]
    public Transform bounceTarget;
    public float bounceAmplitude = 0.1f;
    public float bounceFrequency = 5f;

    private Vector3 initialBouncePos;
    private bool isTyping = false;
    
    public string fullText { get; set; }

    public void StartTyping()
    {
        StartCoroutine(TypeText());
        if (bounceTarget != null)
            initialBouncePos = bounceTarget.localPosition;
    }

    public void ConfirmDialogue()
    {
        GameManager.instance.ConfirmDialogue();
        gameObject.SetActive(false);
    }

    private IEnumerator TypeText()
    {
        isTyping = true;
        textComponent.text = "";

        foreach (char c in fullText)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        GameManager.instance.StopMumble();

        if (bounceTarget != null)
            bounceTarget.localPosition = initialBouncePos;
    }
    
    void Update()
    {
        if (isTyping && bounceTarget != null)
        {
            float offsetY = Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude;
            bounceTarget.localPosition = initialBouncePos + new Vector3(0, offsetY, 0);
        }
    }
}
