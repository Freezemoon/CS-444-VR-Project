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

    private bool _isBaitAtInitPos;
    private bool _isHeld;
    private bool _isHolding;
    private Vector3 _previousPos;
    private Vector3 _handVelocity;
    
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
}
