using System.Collections;
using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

[RequireComponent(typeof(XRGrabInteractable))]
public class Dynamite : MonoBehaviour
{
    // Static cache to avoid repeated FindObjectsOfType calls
    private static Dictionary<InteractorHandedness, XRBaseInteractor> _interactorMap;
    private static bool _mapInitialized;

    [Header("Fuse")] [Tooltip("Time in seconds before explosion after ignition.")] [SerializeField] [Min(0f)]
    private float fuseTime = 3f;

    [Tooltip("Where on the dynamite the fuse starts.")] [SerializeField]
    private Transform fuseStartPoint;

    [Tooltip("Where on the dynamite the fuse ends.")] [SerializeField]
    private Transform fuseEndPoint;

    [Tooltip("Blast radius in world units.")] [SerializeField] [Range(0.1f, 20f)]
    private float blastRadius = 3f;

    [Header("Visual Effects")] [Tooltip("Spawned on explode.")] [SerializeField]
    private GameObject explosionVFX;

    [Tooltip("Spawned on explode.")] [SerializeField]
    private GameObject smokeVFX;

    [Tooltip("Optional fuse-burning VFX.")] [SerializeField]
    private GameObject fuseVFX;

    [Header("Audio")] [Tooltip("Plays while fuse burns.")] [SerializeField]
    private AudioClip fuseSound;

    [Tooltip("Plays on explosion.")] [SerializeField]
    private AudioClip explosionSound;

    [Tooltip("Plays when landing in water.")] [SerializeField]
    private AudioClip waterSound;
    
    [Header("Fish prefabs")] [Tooltip("Fish prefabs that will spawn when exploding fishing zones.")]
    [SerializeField] private GameObject easyFish;
    [SerializeField] private GameObject mediumFish;
    [SerializeField] private GameObject hardFish;

    [Header("Debug")] [Tooltip("Lights the dynamite")] [SerializeField]
    private bool lit;

    [SerializeField] private GameObject zippoPrefab;

    private AudioSource _audioSource;
    private Coroutine _fuseRoutine;
    private XRGrabInteractable _grab;
    private bool _isSinking;
    private Rigidbody _rb;
    private GameObject _zippoInstance;
    private Transform _xrOrigin;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody>();
        _grab = GetComponent<XRGrabInteractable>();
        _grab.selectEntered.AddListener(OnGrabbed);
        _grab.selectExited.AddListener(OnReleased);
        
        var go = GameObject.Find("XR Origin custom");
        if (go == null) Debug.LogError("XR Origin not found!");
        else      _xrOrigin = go.transform;
        
    }

    private void Start()
    {
        if (lit) Ignite();
    }

    private void OnDestroy()
    {
        _grab.selectEntered.RemoveListener(OnGrabbed);
        _grab.selectExited.RemoveListener(OnReleased);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isSinking) return;
        if (other.CompareTag("WaterFishingLimit"))
            StartCoroutine(HandleWaterSink(other));
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        // Lazy-initialize the interactor map once per session
        if (!_mapInitialized)
        {
            _interactorMap = new Dictionary<InteractorHandedness, XRBaseInteractor>();
            foreach (var interactor in FindObjectsOfType<XRBaseInteractor>())
                _interactorMap[interactor.handedness] = interactor;

            _mapInitialized = true;
        }

        // Which hand grabbed the dynamite?
        var grabbingHand = args.interactorObject.handedness;

        // Determine opposite hand
        var otherHand = grabbingHand == InteractorHandedness.Left
            ? InteractorHandedness.Right
            : InteractorHandedness.Left;
        
        
        string handNode = otherHand == InteractorHandedness.Left 
            ? "Left" 
            : "Right";
        
        Transform visual = _xrOrigin
            .Find($"Camera Offset/{handNode} Controller/{handNode} Controller Visual/UniversalController/Controller_Base");

        if (visual == null)
        {
            Debug.LogWarning($"Couldn’t find controller visual game object");
            return;
        }
        
        _zippoInstance = Instantiate(
            zippoPrefab,
            visual.position,
            visual.rotation,
            visual
        );
        _zippoInstance.transform.localPosition = Vector3.zero;
        _zippoInstance.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        // Destroy the spawned Zippo when the dynamite is released
        if (_zippoInstance != null)
        {
            Destroy(_zippoInstance);
            _zippoInstance = null;
        }
    }

    private IEnumerator HandleWaterSink(Collider water)
    {
        _isSinking = true;

        // Play splash sound
        if (waterSound)
            AudioSource.PlayClipAtPoint(waterSound, transform.position);

        // Freeze at the water surface
        _rb.isKinematic = true;
        var pos = transform.position;
        var surfY = water.transform.position.y;
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
    ///     Starts the fuse and schedules the explosion.
    /// </summary>
    public void Ignite()
    {
        if (_fuseRoutine != null) return; // already lit
        
        GameManager.instance.SetDialogueState(GameManager.DialogueState.ThrowDynamite);

        // spawn & move the fuse VFX
        if (fuseVFX && fuseStartPoint != null && fuseEndPoint != null)
        {
            var fuseGo = Instantiate(fuseVFX, fuseStartPoint.position, fuseStartPoint.rotation, transform);
            // force it to the exact start-local position
            fuseGo.transform.localPosition = fuseStartPoint.localPosition;
            _ = StartCoroutine(MoveFuseVFX(fuseGo.transform));
        }

        // play fuse sound
        if (fuseSound)
            _audioSource.PlayOneShot(fuseSound);

        // schedule explosion
        _fuseRoutine = StartCoroutine(FuseCountdown());
    }

    private IEnumerator MoveFuseVFX(Transform fx)
    {
        var start = fuseStartPoint.localPosition;
        var end = fuseEndPoint.localPosition;
        var elapsed = 0f;

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

    // ReSharper disable Unity.PerformanceAnalysis
    /// <summary>
    ///     Triggers the explosion VFX, sound, and destroys the dynamite.
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
                var difficulty = hit.GetComponent<FishingArea>().areaDifficulty;
                SpawnFishes(difficulty, hit.transform.position);
                Destroy(hit.gameObject);
            }

            if (hit.TryGetComponent<Explodable>(out var explodable))
            {
                explodable.PlaySound();
                Destroy(explodable.gameObject);
            }
        }

        Destroy(gameObject);
    }

    private void SpawnFishes(FishingGame.Difficulty difficulty, Vector3 origin)
    {
        float radialVelocity = 3f;
        float upwardVelocity = 3f;
        
        // pick correct fish prefab
        GameObject prefab;
        switch (difficulty)
        {
            case FishingGame.Difficulty.Easy:   prefab = easyFish;   break;
            case FishingGame.Difficulty.Medium: prefab = mediumFish; break;
            case FishingGame.Difficulty.Hard:   prefab = hardFish;   break;
            default:                            prefab = easyFish;   break;
        }
        

        // spawn at the origin
        var fish = Instantiate(prefab, origin, Quaternion.identity);

        // send it flying into the air with some randomness
        if (fish.TryGetComponent<Rigidbody>(out var rbody))
        {
            // a little variation on upward speed
            float up = upwardVelocity * Random.Range(0.8f, 1.2f);

            // pick a random horizontal direction
            var dir2d = Random.insideUnitCircle.normalized;
            var side  = new Vector3(dir2d.x, 0f, dir2d.y) 
                        * radialVelocity
                        * Random.Range(0.8f, 1.2f);

            rbody.linearVelocity = new Vector3(side.x, up, side.z);
            
            rbody.AddTorque(Random.insideUnitSphere * radialVelocity, ForceMode.Impulse);
        }

        // Randomly spawn some easy fish too
        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            var easyFish = Instantiate(this.easyFish, origin, Quaternion.identity);
            
            if (easyFish.TryGetComponent<Rigidbody>(out var rb))
            {
                float up = upwardVelocity * Random.Range(0.8f, 1.2f);
                
                var dir2d = Random.insideUnitCircle.normalized;
                var side  = new Vector3(dir2d.x, 0f, dir2d.y) 
                            * radialVelocity
                            * Random.Range(0.8f, 1.2f);

                rb.linearVelocity = new Vector3(side.x, up, side.z);
                
                rb.AddTorque(Random.insideUnitSphere * radialVelocity, ForceMode.Impulse);
            }
        }
    }
}