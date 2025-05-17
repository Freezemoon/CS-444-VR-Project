using UnityEngine;

public class Explodable : MonoBehaviour
{
    [SerializeField] private AudioClip destroySfx;

    private void OnDestroy()
    {
        if (destroySfx != null)
            AudioSource.PlayClipAtPoint(destroySfx, transform.position);
    }
}
