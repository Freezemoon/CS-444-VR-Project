using System;
using System.Collections;
using Game;
using UnityEngine;

/// <summary>
/// Bucket socket interaction. It works by detecting a collision with a GameObject with "Fish" tag, duplicate it for
/// the animation and destroys the original to remove it from player's hands. It will also work when the fish is
/// thrown into it.
/// </summary>
public class FishBucket : MonoBehaviour
{
    public static event Action NewFish;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Update the bucket value properly as well as the type cought
        if (!other.CompareTag("Fish"))
            return;

        GameManager.instance.HandleCaughtFish(other.GetComponent<GrabFish>().difficulty);

        GameManager.instance.SetDialogueState(GameManager.DialogueState.HopOnBoat);
        FishingGame.instance.ResetGameWhenFishIsGrabbedByUser();
        
        // Duplicate the fish
        GameObject duplicate = Instantiate(other.gameObject);
        
        // Destroy the original fish
        Destroy(other.gameObject);

        // Strip down the duplicate to visuals only
        ToKinetic(duplicate);

        NewFish?.Invoke();

        // End with the socket animation
        StartCoroutine(SocketAnimation(duplicate));
    }
    
    // Animates the given GameObject then destroys it.
    private IEnumerator SocketAnimation(GameObject fish)
    {
        // Set initial position and rotation
        fish.transform.position = transform.position - Vector3.down * 0.2f;
        fish.transform.rotation = Quaternion.Euler(0f, 0f, -90f);

        float duration = 1f;
        float height = 0.6f;

        Vector3 basePos = transform.position;
        Vector3 startScale = fish.transform.localScale;
        Vector3 endScale = startScale * (1f * 0.4f);

        float elapsed = 0f;

        bool audioPlayed = false;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            float yOffset = Mathf.Sin(Mathf.PI * t) * height;
            fish.transform.position = basePos + Vector3.up * (yOffset - 0.2f);

            // Shrink during descent (second half)
            if (t > 0.5f)
            {
                float shrinkT = (t - 0.5f) * 2f; // Normalize to 0–1 over second half
                fish.transform.localScale = Vector3.Lerp(startScale, endScale, shrinkT);
            }
            
            // SFX
            if (t >= 0.55 && !audioPlayed)
            {
                _audioSource.Play();
                audioPlayed = true;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(fish);
    }

    // Helper to remove unecessary components from duplicate
    private void ToKinetic(GameObject fish)
    {
        // Remove or change tag to prevent re-triggering
        fish.tag = "Untagged";
        
        // disable rigidbody if any
        var rb = fish.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }
    
}
