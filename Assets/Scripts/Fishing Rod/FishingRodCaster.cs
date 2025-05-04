using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FishingRodCaster : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference castAction; // Reference to trigger action (Press & Release)
    public XRGrabInteractable rodInteractable;
    public Collider rodCollider;

    [Header("Transforms")]
    public Transform controllerTransform;  // The hand/controller transform
    public Transform baitTransform;
    public Transform baitInitPosTransform;
    public Transform baitParentTransform;
    
    [Header("Others")]
    public Rigidbody baitRb;

    [Header("Casting Settings")]
    public float castForceMultiplier = 1.5f;
    
    [Header("Hook Detection")]
    public float hookPitchThreshold = 500f; // Degrees per second
    public AudioSource hookSetAudio;

    private bool _canTriggerPull = true;
    private float _pullCooldown = 1.0f;
    private float _lastPullTime;
    
    private bool _isBaitAtInitPos;
    private bool _isHeld;
    private bool _isHolding;
    private Vector3 _previousPos;
    private Vector3 _handVelocity;
    private Quaternion _prevHandRotation;
    private float _backwardPitchSpeed;
    
    private System.Action<InputAction.CallbackContext> _onCastStarted;
    private System.Action<InputAction.CallbackContext> _onCastCanceled;

    private void Start()
    {
        OnReelReachedMinLength();
    }

    private void OnEnable()
    {
        // Store callbacks so we can unsubscribe later
        _onCastStarted = _ => StartHoldingToThrowBait();
        _onCastCanceled = _ => ReleaseAndCastBait();
        
        castAction.action.started += _onCastStarted;
        castAction.action.canceled += _onCastCanceled;

        GrabRotateAroundPivot.OnReelReachedMinLength += OnReelReachedMinLength;
        
        rodInteractable.selectEntered.AddListener(OnGrabbedFishingRod);
        rodInteractable.selectExited.AddListener(OnReleasedFishingRod);

        castAction.action.Enable();
    }

    private void OnDisable()
    {
        castAction.action.started -= _onCastStarted;
        castAction.action.canceled -= _onCastCanceled;
        
        GrabRotateAroundPivot.OnReelReachedMinLength -= OnReelReachedMinLength;
        
        rodInteractable.selectEntered.RemoveListener(OnGrabbedFishingRod);
        rodInteractable.selectExited.RemoveListener(OnReleasedFishingRod);
    }

    private void Update()
    {
        UpdateHandVelocity();
        
        UpdateHandPitchRotation();
        DetectHookPull();
    }

    private void OnReelReachedMinLength()
    {
        baitRb.isKinematic = true;
        baitRb.linearVelocity = Vector3.zero;

        _isBaitAtInitPos = true;

        _previousPos = controllerTransform.position;
        
        baitTransform.SetParent(transform);
        baitTransform.position = baitInitPosTransform.position;
    }
    
    private void OnGrabbedFishingRod(SelectEnterEventArgs args)
    {
        _isHeld = true;
        // Disables collider which avoids unintentional grab with the other hand
        rodCollider.enabled = false;
    }

    private void OnReleasedFishingRod(SelectExitEventArgs args)
    {
        _isHeld = false;
        // Disables collider which avoids unintentional grab with the other hand
        rodCollider.enabled = true;
    }

    private void UpdateHandVelocity()
    {
        Vector3 current = controllerTransform.position;
        _handVelocity = (current - _previousPos) / Time.deltaTime;
        _previousPos = current;
    }

    private void StartHoldingToThrowBait()
    {
        if (!_isHeld) return;
        if (!_isBaitAtInitPos) return;
        
        _isHolding = true;
        
        baitRb.isKinematic = true;
        baitRb.linearVelocity = Vector3.zero;
        
        baitTransform.SetParent(transform);
        baitTransform.position = baitInitPosTransform.position;
    }

    private void ReleaseAndCastBait()
    {
        if (!_isHeld) return;
        if (!_isHolding) return;
        
        _isHolding = false;
        _isBaitAtInitPos = false;
        
        baitTransform.SetParent(baitParentTransform);
        baitTransform.position = baitInitPosTransform.position;
        
        baitRb.isKinematic = false;
        baitRb.linearVelocity = _handVelocity * castForceMultiplier;
    }
    
    private void UpdateHandPitchRotation()
    {
        Quaternion currentRotation = controllerTransform.rotation;
        Quaternion delta = currentRotation * Quaternion.Inverse(_prevHandRotation);

        delta.ToAngleAxis(out float angle, out Vector3 worldAxis);
        _prevHandRotation = currentRotation;

        if (angle > 180f) angle -= 360f;

        // Project world axis to local space of the controller
        Vector3 localAxis = controllerTransform.InverseTransformDirection(worldAxis);

        // We're interested in rotation around local X (pitch)
        bool isPitch = Mathf.Abs(localAxis.x) > 0.7f && Mathf.Abs(localAxis.y) < 0.4f && Mathf.Abs(localAxis.z) < 0.4f;

        // If rotating around pitch and in the backward direction (positive X)
        if (isPitch && localAxis.x < 0f)
        {
            _backwardPitchSpeed = Mathf.Abs(angle) / Time.deltaTime;
        }
        else
        {
            _backwardPitchSpeed = 0f;
        }
    }
    
    private void DetectHookPull()
    {
        if (!_isHeld) return;
        
        if (Time.time - _lastPullTime >= _pullCooldown)
        {
            _canTriggerPull = true;
        }

        if (!_canTriggerPull) return;
        
        // Debug
        // FishingGame.instance.tmp_text.text = _backwardPitchSpeed.ToString();
        
        // Only trigger if the angular speed is a fast upward (backward) flick
        if (_backwardPitchSpeed > hookPitchThreshold)
        {
            // hookSetAudio?.Play();
            FishingGame.instance.PullSuccess();
            _canTriggerPull = false;
            _lastPullTime = Time.time;
        }
    }
}
