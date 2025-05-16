using System.Collections;
using UnityEngine;

public class Dynamite : MonoBehaviour
{
    [Header("Fuse")]
    [Tooltip("Time in seconds before explosion after ignition.")]
    [SerializeField, Min(0f)]
    private float fuseTime = 3f;
    [Tooltip("Where on the dynamite the fuse starts.")]
    [SerializeField] private Transform fuseStartPoint;
    [Tooltip("Where on the dynamite the fuse ends.")]
    [SerializeField] private Transform fuseEndPoint;

    [Tooltip("Blast radius in world units.")]
    [SerializeField, Range(0.1f, 20f)] 
    private float blastRadius = 3f;

    [Header("Visual Effects")]
    [Tooltip("Spawned on explode.")]
    [SerializeField] private GameObject explosionVFX;
    [Tooltip("Spawned on explode.")]
    [SerializeField] private GameObject smokeVFX;
    [Tooltip("Optional fuse-burning VFX.")]
    [SerializeField] private GameObject fuseVFX;

    [Header("Audio")]
    [Tooltip("Plays while fuse burns.")]
    [SerializeField] private AudioClip fuseSound;
    [Tooltip("Plays on explosion.")]
    [SerializeField] private AudioClip explosionSound;
    [Tooltip("Plays when landing in water.")]
    [SerializeField] private AudioClip waterSound;

    [Header("Debug")] [Tooltip("Lights the dynamite")] [SerializeField]
    private bool lit;

    private AudioSource _audioSource;
    private Coroutine _fuseRoutine;
    private Rigidbody _rb;
    private bool _isSinking;
    
    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb          = GetComponent<Rigidbody>();
    }
    
    void Start()
    {
        if (lit) Ignite();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (_isSinking) return;
        if (other.CompareTag("Water"))
            StartCoroutine(HandleWaterSink(other));
    }
    
    private IEnumerator HandleWaterSink(Collider water)
    {
        _isSinking = true;

        // Play splash sound
        if (waterSound)
            AudioSource.PlayClipAtPoint(waterSound, transform.position);

        // Freeze at the water surface
        _rb.isKinematic = true;
        Vector3 pos = transform.position;
        float surfY = water.transform.position.y;
        transform.position = new Vector3(pos.x, surfY, pos.z);

        // Wait before sinking
        yield return new WaitForSeconds(0.15f);

        // Sink slowly
        while (true)
        {
            transform.position += Vector3.down * (0.65f * Time.deltaTime);
            yield return null;
        }
    }
    
    /// <summary>
    /// Starts the fuse and schedules the explosion.
    /// </summary>
    public void Ignite()
    {
        if (_fuseRoutine != null) return;  // already lit

        // 1) spawn & move the fuse VFX
        if (fuseVFX && fuseStartPoint != null && fuseEndPoint != null)
        {
            var fuseGo = Instantiate(fuseVFX, fuseStartPoint.position, fuseStartPoint.rotation, transform);
            // force it to the exact start-local position
            fuseGo.transform.localPosition = fuseStartPoint.localPosition;
            _ = StartCoroutine(MoveFuseVFX(fuseGo.transform));
        }

        // 2) play fuse sound
        if (fuseSound)
            _audioSource.PlayOneShot(fuseSound);

        // 3) schedule explosion
        _fuseRoutine = StartCoroutine(FuseCountdown());
    }
    
    private IEnumerator MoveFuseVFX(Transform fx)
    {
        Vector3 start = fuseStartPoint.localPosition;
        Vector3 end   = fuseEndPoint.localPosition;
        float elapsed = 0f;

        while (elapsed < fuseTime)
        {
            fx.localPosition = Vector3.Lerp(start, end, elapsed / fuseTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        // ensure it ends exactly at the tip
        fx.localPosition = end;
        Destroy(fx.gameObject);
    }
    
    private IEnumerator FuseCountdown()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }
    
    /// <summary>
    /// Triggers the explosion VFX, sound, and destroys the dynamite.
    /// </summary>
    public void Explode()
    {
        
        if (explosionVFX)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        if (explosionSound)
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, 1f);

        if (smokeVFX)
            Instantiate(smokeVFX, transform.position, Quaternion.identity);
        
        var hits = Physics.OverlapSphere(transform.position, blastRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("FishingZone"))
            {
                Destroy(hit.gameObject);
            }
        }
        
        Destroy(gameObject);
    }
}
