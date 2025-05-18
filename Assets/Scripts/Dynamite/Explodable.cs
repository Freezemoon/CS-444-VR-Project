using UnityEngine;

public class Explodable : MonoBehaviour
{
    [SerializeField] private AudioClip destroySfx;

    public void PlaySound()
    {
        if (destroySfx != null)
            AudioSource.PlayClipAtPoint(destroySfx, transform.position);
    }
}
