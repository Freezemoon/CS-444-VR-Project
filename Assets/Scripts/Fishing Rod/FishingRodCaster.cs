using Game;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FishingRodCaster : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference castAction; // Reference to trigger action (Press & Release)
    public InputActionReference aButtonAction;
    public XRGrabInteractable rodInteractable;

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
    public AudioSource pullSuccessAudioSource;

    private bool _canTriggerPull = false;
    private readonly float _pullCooldown = 1.0f;
    private float _lastPullTime;
    
    private bool _isBaitAtInitPos;
    private bool _isHeld;
    private bool _isHolding;
    private Vector3 _previousPos;
    private Vector3 _handVelocity;
    private Quaternion _prevHandRotation;
    private float _backwardPitchSpeed;
    
    private void Start()
    {
        OnReelReachedMinLength();
    }

    private void OnEnable()
    {
        castAction.action.started += StartHoldingToThrowBait;
        castAction.action.canceled += ReleaseAndCastBait;
        aButtonAction.action.started += OnAButtonPressed;

        GrabRotateAroundPivot.OnReelReachedMinLength += OnReelReachedMinLength;
        
        rodInteractable.selectEntered.AddListener(OnGrabbedFishingRod);
        rodInteractable.selectExited.AddListener(OnReleasedFishingRod);

        castAction.action.Enable();
    }

    private void OnDisable()
    {
        castAction.action.started -= StartHoldingToThrowBait;
        castAction.action.canceled -= ReleaseAndCastBait;
        aButtonAction.action.started -= OnAButtonPressed;
        
        GrabRotateAroundPivot.OnReelReachedMinLength -= OnReelReachedMinLength;
        
        rodInteractable.selectEntered.RemoveListener(OnGrabbedFishingRod);
        rodInteractable.selectExited.RemoveListener(OnReleasedFishingRod);
    }

    private void Update()
    {
        FishingGame.instance.canStart = !baitRb.isKinematic;
        
        UpdateHandVelocity();
        
        UpdateHandPitchRotation();
        DetectRodPull();
    }

    private void OnReelReachedMinLength()
    {
        baitRb.isKinematic = true;

        _isBaitAtInitPos = true;

        _previousPos = controllerTransform.position;
        
        baitTransform.SetParent(transform);
        baitTransform.position = baitInitPosTransform.position;
    }
    
    private void OnGrabbedFishingRod(SelectEnterEventArgs args)
    {
        GameManager.instance.SetDialogueState(GameManager.DialogueState.FishingRodGrabbed);
        _isHeld = true;
    }

    private void OnReleasedFishingRod(SelectExitEventArgs args)
    {
        _isHeld = false;
    }

    private void UpdateHandVelocity()
    {
        Vector3 current = controllerTransform.position;
        _handVelocity = (current - _previousPos) / Time.deltaTime;
        _previousPos = current;
    }
    
    private void OnAButtonPressed(InputAction.CallbackContext context)
    {
        transform.position = controllerTransform.position;
        transform.rotation = Quaternion.Euler(Vector3.right * -90f);
        
        baitTransform.position = baitInitPosTransform.position;
    }

    private void StartHoldingToThrowBait(InputAction.CallbackContext context)
    {
        if (!_isHeld) return;
        if (!_isBaitAtInitPos) return;
        if (FishingGame.instance.canGrabFish) return;
        
        _isHolding = true;
        
        baitRb.isKinematic = true;
        
        baitTransform.SetParent(transform);
        baitTransform.position = baitInitPosTransform.position;
    }

    private void ReleaseAndCastBait(InputAction.CallbackContext context)
    {
        if (!_isHeld) return;
        if (!_isHolding) return;
        
        GameManager.instance.SetDialogueState(GameManager.DialogueState.Reel);
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
        Quaternion deltaCurrAndPrev = currentRotation * Quaternion.Inverse(_prevHandRotation);

        deltaCurrAndPrev.ToAngleAxis(out float angle, out Vector3 worldAxis);
        _prevHandRotation = currentRotation;

        if (angle > 180f) angle -= 360f;

        // Convert the world axis to the controller’s local space
        Vector3 localAxis = controllerTransform.InverseTransformDirection(worldAxis);

        // We're interested in rotation around local X (pitch)
        bool isPitch = Mathf.Abs(localAxis.x) > 0.7f && Mathf.Abs(localAxis.y) < 0.4f && Mathf.Abs(localAxis.z) < 0.4f;

        // If rotating around pitch and in the backward direction (negative X)
        if (isPitch && localAxis.x < 0f)
        {
            _backwardPitchSpeed = Mathf.Abs(angle) / Time.deltaTime;
        }
        else
        {
            _backwardPitchSpeed = 0f;
        }
    }
    
    private void DetectRodPull()
    {
        if (!_isHeld) return;
        
        if (FishingGame.instance.gameState == FishingGame.GameState.Pulling &&
            Time.time - _lastPullTime >= _pullCooldown)
        {
            _canTriggerPull = true;
        }

        if (!_canTriggerPull) return;
        
        // Only trigger if the angular speed is a fast upward (backward) flick
        if (_backwardPitchSpeed > hookPitchThreshold)
        {
            pullSuccessAudioSource?.Play();
            FishingGame.instance.PullSuccess();
            
            _canTriggerPull = false;
            _lastPullTime = Time.time;
        }
    }
}
