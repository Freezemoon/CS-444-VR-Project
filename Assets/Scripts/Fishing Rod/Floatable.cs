using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Floatable : MonoBehaviour
{
    [Tooltip("Height above the water surface to float at")]
    public float floatHeight = -0.1f;
    [Tooltip("How stiffly it returns to floatHeight (bigger = snappier)")]
    public float bounceStrength = 3f;
    
    [Tooltip("Plays when landing in water.")] [SerializeField]
    private AudioClip waterSound;

    Rigidbody _rb;
    bool      _inWater;
    float     _waterY;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        // make sure it’s non-kinematic so physics runs
        _rb.isKinematic = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("WaterFishingLimit")) return;
        
        if (waterSound)
            AudioSource.PlayClipAtPoint(waterSound, transform.position);

        // grab surface Y and stop all current motion
        _waterY       = other.bounds.max.y;
        _inWater      = true;
        _rb.useGravity = false;
        _rb.linearVelocity   = Vector3.zero;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("WaterFishingLimit")) return;
        _inWater       = false;
        _rb.useGravity = true;
    }

    // Bouncy behavior floating back up that slowly "converges to a stop".
    void FixedUpdate()
    {
        if (!_inWater) return;

        // target height
        float targetY      = _waterY + floatHeight;
        // displacement from equilibrium
        float displacement = targetY - transform.position.y;
        // simple spring force
        float springForce  = displacement * bounceStrength;
        // damping proportional to vertical velocity
        float dampingForce = -_rb.linearVelocity.y * (bounceStrength * 0.12f);

        // net upward acceleration
        Vector3 force = Vector3.up * (springForce + dampingForce);
        _rb.AddForce(force, ForceMode.Acceleration);
    }

}