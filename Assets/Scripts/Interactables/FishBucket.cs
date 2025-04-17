using System.Collections;
using UnityEngine;

/// <summary>
/// Bucket socket interaction. It works by detecting a collision with a GameObject with "Fish" tag, duplicate it for
/// the animation and destroys the original to remove it from player's hands. It will also work when the fish is
/// thrown into it.
/// </summary>
public class FishBucket : MonoBehaviour
{
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Fish"))
            return;

        // Duplicate the fish
        GameObject duplicate = Instantiate(other.gameObject);
        
        // Destroy the original fish
        Destroy(other.gameObject);

        // Strip down the duplicate to visuals only
        ToKinetic(duplicate);

        // End with the socket animation
        StartCoroutine(SocketAnimation(duplicate));
        
        Debug.Log("Nice catch!");
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
        
        // Disable all scripts
        foreach (var script in fish.GetComponents<MonoBehaviour>())
        {
            script.enabled = false;
        }
        
        // disable all colliders
        foreach (var col in fish.GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        
        // disable rigidbody if any
        var rb = fish.GetComponent<Rigidbody>();
        if (rb != null)
            rb.isKinematic = true;
    }
    
}
