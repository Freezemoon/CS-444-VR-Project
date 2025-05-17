using System.Collections;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class TypewriterEffect : MonoBehaviour
{
    public TMP_Text textComponent;
    public float typingSpeed = 0.1f;
    public GameObject canvas;

    [Header("Bouncing Target")]
    public Transform larryMustache;
    public float bounceAmplitude = 0.1f;
    public float bounceFrequency = 5f;

    private Vector3 initialBouncePos;
    private bool isTyping = false;

    private Coroutine coroutine;
    
    public string fullText { get; set; }

    public void StartTyping()
    {
        if (coroutine != null) StopCoroutine(coroutine);
        coroutine = StartCoroutine(TypeText());
        
        if (larryMustache != null)
            initialBouncePos = larryMustache.localPosition;
    }

    public void ConfirmDialogue()
    {
        GameManager.instance.ConfirmDialogue();
        canvas.SetActive(false);
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

        if (larryMustache != null)
            larryMustache.localPosition = initialBouncePos;
    }
    
    void Update()
    {
        if (isTyping && larryMustache != null)
        {
            float offsetY = Mathf.Sin(Time.time * bounceFrequency) * bounceAmplitude;
            larryMustache.localPosition = initialBouncePos + new Vector3(0, offsetY, 0);
        }
    }
}
