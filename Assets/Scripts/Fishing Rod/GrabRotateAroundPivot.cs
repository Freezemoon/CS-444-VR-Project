using System;
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
    public float reelForceMultiplier = 0.005f;
    public float lockedLineLengthMax = 8f;
    public float lineLengthMin = 0.1f;
    
    public static event Action OnReelReachedMinLength;

    private readonly Vector3 _localNormalPlanePivot = Vector3.up;

    private Transform _interactorAttachTransform;
    private XRGrabInteractable _handlerGrab;

    private float _grabbedRadius;

    private Vector3 _previousDirectionOnPlane;
    
    private float _lockedLineLength;

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
        _lockedLineLength = lockedLineLengthMax;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        _interactorAttachTransform = args.interactorObject.GetAttachTransform(_handlerGrab);

        Vector3 planeNormal = handlerPivotTransform.TransformDirection(_localNormalPlanePivot).normalized;
        Vector3 toStick = transform.position - handlerPivotTransform.position;
        Vector3 projected = Vector3.ProjectOnPlane(toStick, planeNormal);

        _grabbedRadius = projected.magnitude;
        
        _lockedLineLength = Vector3.Distance(baitRigidbody.position, pullTowardTransform.position);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        _interactorAttachTransform = null;
        _lockedLineLength = lockedLineLengthMax;
    }

    private void Update()
    {
        Vector3 toBait = baitRigidbody.position - pullTowardTransform.position;

        if (_interactorAttachTransform && handlerPivotTransform)
        {
            Vector3 planeNormal = handlerPivotTransform.TransformDirection(_localNormalPlanePivot).normalized;

            // Vector from pivot to current hand position, projected on plane
            Vector3 toHand = _interactorAttachTransform.position - handlerPivotTransform.position;
            Vector3 projectedHand = Vector3.ProjectOnPlane(toHand, planeNormal).normalized;
            UpdateRotation(planeNormal, projectedHand);
        
            float angleDelta = Vector3.SignedAngle(_previousDirectionOnPlane, projectedHand, planeNormal);
            UpdateReelForce(angleDelta, toBait);

            if (_lockedLineLength <= lineLengthMin * 1.1f)
            {
                OnReelReachedMinLength?.Invoke();
            }
            
            _previousDirectionOnPlane = projectedHand;
        }
        
        UpdateReelMaxDistance(toBait);
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

    private void UpdateReelForce(float angleDelta, Vector3 toBait)
    {
        angleDelta = Mathf.Clamp(angleDelta, 0, 20f);
        // If actively reeling, shorten the rope
        if (Mathf.Abs(angleDelta) < 0.1f)
        {
            return;
        }
        
        // Pull inward
        Vector3 directionToTarget = -toBait.normalized;
        float forceAmount = angleDelta * reelForceMultiplier;
        baitRigidbody.linearVelocity += directionToTarget * forceAmount;
    
        // shorten locked line length slightly (if you want to simulate reeling in)
        _lockedLineLength = Mathf.Max(_lockedLineLength - Mathf.Abs(forceAmount), lineLengthMin); // Prevent zero length
    }

    private void UpdateReelMaxDistance(Vector3 toBait)
    {
        float currentDistance = toBait.magnitude;
        
        // Clamp if it's stretching beyond the current rope length
        if (currentDistance <= _lockedLineLength)
        {
            return;
        }
        Vector3 clampedPos = pullTowardTransform.position + toBait.normalized * _lockedLineLength;
        baitRigidbody.position = clampedPos;

        Vector3 outwardVelocity = Vector3.Project(baitRigidbody.linearVelocity, toBait.normalized);
        baitRigidbody.linearVelocity -= outwardVelocity;
    }
}
