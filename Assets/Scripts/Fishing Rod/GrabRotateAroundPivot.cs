using System;
using Game;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class GrabRotateAroundPivot : MonoBehaviour
{
    public Transform handlerPivotTransform;
    public Rigidbody baitRigidbody;
    public Transform pullTowardTransform;
    public float reelForceMultiplier = 0.002f;
    public float lineLengthMin = 0.1f;
    public AudioSource reelAudioSource;

    public ConfigurableJoint baitJoint;
    public Transform rodTip;
    
    public static event Action OnReelReachedMinLength;
    public float currentLockedLineLengthMax { get; private set; }

    private readonly Vector3 _localNormalPlanePivot = Vector3.up;

    private Transform _interactorAttachTransform;
    private XRGrabInteractable _handlerGrab;

    private float _grabbedRadius;

    private Vector3 _previousDirectionOnPlane;
    
    private float _currentReelForceMultiplier;
    
    private readonly float _lockedLineLengthMax = 10f;
    private readonly float _lockedLineLengthMaxAddForGame = 8f;
    
    private Vector3 _toHandPrev;

    private void OnEnable()
    {
        _handlerGrab = GetComponent<XRGrabInteractable>();
        _handlerGrab.selectEntered.AddListener(OnGrab);
        _handlerGrab.selectExited.AddListener(OnRelease);
    }

    private void OnDisable()
    {
        _handlerGrab.selectEntered.RemoveListener(OnGrab);
        _handlerGrab.selectExited.RemoveListener(OnRelease);
    }

    private void Start()
    {
        Vector3 planeNormal = handlerPivotTransform.TransformDirection(_localNormalPlanePivot).normalized;
        Vector3 toStick = transform.position - handlerPivotTransform.position;
        Vector3 projected = Vector3.ProjectOnPlane(toStick, planeNormal);

        _grabbedRadius = projected.magnitude;
        
        Vector3 projectedHand = Vector3.ProjectOnPlane(Vector3.up, planeNormal).normalized;
        UpdateRotation(planeNormal, projectedHand);
        
        currentLockedLineLengthMax = _lockedLineLengthMax;
        
        baitJoint.connectedAnchor = rodTip.position;
        SetLineLength(lineLengthMin);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        _interactorAttachTransform = args.interactorObject.GetAttachTransform(_handlerGrab);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        _interactorAttachTransform = null;
        reelAudioSource.Pause();
        
        Vector3 planeNormal = handlerPivotTransform.TransformDirection(_localNormalPlanePivot).normalized;
        Vector3 projectedHand = Vector3.ProjectOnPlane(_toHandPrev, planeNormal).normalized;
        UpdateRotation(planeNormal, projectedHand);
        
        SetLineLength(currentLockedLineLengthMax);
    }

    private void Update()
    {
        baitJoint.connectedAnchor = rodTip.position;
        
        UpdateMaxLineLengthAndReelForceMultiplier();

        UpdateLineLength();
    }
    
    public void SetLineLength(float length)
    {
        SoftJointLimit limit = new SoftJointLimit { limit = length };
        baitJoint.linearLimit = limit;
    }

    private void UpdateMaxLineLengthAndReelForceMultiplier()
    {
        // If game is started the max length of the line and the reel force are increased 
        if (FishingGame.instance.fishingGameState == FishingGame.FishingGameState.Pulling ||
            FishingGame.instance.fishingGameState == FishingGame.FishingGameState.Reeling)
        {
            currentLockedLineLengthMax = _lockedLineLengthMax + _lockedLineLengthMaxAddForGame;
            _currentReelForceMultiplier = reelForceMultiplier / FishingGame.instance.reelForceMultiplierDivisor;
        }
        else
        {
            currentLockedLineLengthMax = _lockedLineLengthMax;
            _currentReelForceMultiplier = reelForceMultiplier;
        }
    }

    private void UpdateLineLength()
    {
        Vector3 toBait = baitRigidbody.position - pullTowardTransform.position;

        if (_interactorAttachTransform)
        {
            // Vector from pivot to current hand position, projected on plane
            _toHandPrev = _interactorAttachTransform.position - handlerPivotTransform.position;
            Vector3 planeNormal = handlerPivotTransform.TransformDirection(_localNormalPlanePivot).normalized;
            Vector3 projectedHand = Vector3.ProjectOnPlane(_toHandPrev, planeNormal).normalized;
            UpdateRotation(planeNormal, projectedHand);
        
            float angleDelta = Vector3.SignedAngle(_previousDirectionOnPlane, projectedHand, planeNormal);
            CheckLineLength(angleDelta, toBait);

            bool canReachMinLength = FishingGame.instance.fishingGameState == FishingGame.FishingGameState.NotStarted || 
                                     FishingGame.instance.fishingGameState == FishingGame.FishingGameState.Win;
            
            float currentLineLength = Vector3.Distance(baitRigidbody.position, pullTowardTransform.position);
            if (canReachMinLength && currentLineLength <= lineLengthMin * 1.1f)
            {
                GameManager.instance.SetDialogueState(GameManager.DialogueState.AimBubble);
                OnReelReachedMinLength?.Invoke();
        
                ReleaseFishingRodHandleIfGrabbed();
                SetLineLength(lineLengthMin);
            }
            
            _previousDirectionOnPlane = projectedHand;
        }
    }

    private void UpdateRotation(Vector3 planeNormal, Vector3 finalDirection)
    {
        // Set final position using original radius and plane
        Vector3 targetPosition = handlerPivotTransform.position + finalDirection * _grabbedRadius +
                                 planeNormal * (transform.localScale.y / 2);
        transform.position = targetPosition;

        // Stick faces outward from pivot
        transform.rotation = Quaternion.LookRotation(finalDirection, planeNormal);
    }

    private void CheckLineLength(float angleDelta, Vector3 toBait)
    {
        if (FishingGame.instance.fishingGameState == FishingGame.FishingGameState.Pulling)
        {
            SetLineLength(currentLockedLineLengthMax);
        }
        
        // Clamp the angle delta to prevent it from going too far
        angleDelta = Mathf.Clamp(angleDelta, 0, 30f);
        // If the angle is small, don't pull inward
        if (Mathf.Abs(angleDelta) < 0.1f)
        {
            reelAudioSource.Pause();
            return;
        }
        
        // If the angle is large, pull inward
        if (!reelAudioSource.isPlaying)
        {
            reelAudioSource.Play();
        }
        Vector3 directionToTarget = -toBait.normalized;
        float forceAmount = angleDelta * _currentReelForceMultiplier;
        baitRigidbody.linearVelocity += directionToTarget * forceAmount;
        
        // If the bait has reeled in enough in fishing game, release the handle
        if (FishingGame.instance.ReelSuccess(forceAmount))
        {
            ReleaseFishingRodHandleIfGrabbed();
        }
        else
        {
            // shorten locked line length slightly (if you want to simulate reeling in)
            float currentLineLength = Vector3.Distance(baitRigidbody.position, pullTowardTransform.position);
            float newLimit = Mathf.Max(currentLineLength - Mathf.Abs(forceAmount), lineLengthMin);
            SetLineLength(newLimit);
        }
    }
    
    private void ReleaseFishingRodHandleIfGrabbed()
    {
        if (!_handlerGrab.isSelected) return;
        
        // Get the first interactor selecting the XRGrabInteractable
        var interactor = _handlerGrab.firstInteractorSelecting;

        // Get the interaction manager from the XRGrabInteractable
        var manager = _handlerGrab.interactionManager;

        if (interactor != null && manager)
        {
            manager.SelectExit(interactor, _handlerGrab);
        }
    }
}
