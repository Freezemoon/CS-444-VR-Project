using Game;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BoatHandle : MonoBehaviour
{
    public Transform boat;
    public Transform handlePivot;
    public XRGrabInteractable grabInteractable;
    public InputActionReference throttleAction; // Trigger button
    public Transform throttleButtonVisual; // Visual button to move
    public float rotationSpeed = 250f;
    public float maxRotation = 45f;
    public float maxButtonYMovement = 0.2f;
    public System.Action<float> OnThrottleChanged; // Notify the Boat script

    private Transform _grabbingHand;
    private float _currentYRotation;
    private Vector3 _localOffset;
    private float _triggerValue;
    private float _initialGrabAngle;
    private float _initialHandleRotation;
    private Vector3 _buttonPosition;

    private void Start()
    {
        _localOffset = boat.InverseTransformPoint(transform.position);
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        throttleAction.action.Enable();

        _buttonPosition = throttleButtonVisual.localPosition;
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
        throttleAction.action.Disable();
    }

    private void FixedUpdate()
    {
        if (!_grabbingHand)
        {
            transform.rotation = Quaternion.Euler(0f, _currentYRotation, 0f) * boat.rotation;
            transform.position = boat.TransformPoint(_localOffset);

            _triggerValue = 0f;
            OnThrottleChanged?.Invoke(_triggerValue);

            throttleButtonVisual.localPosition = _buttonPosition;
            return;
        }

        Vector3 handLocal = boat.InverseTransformPoint(_grabbingHand.position);
        Vector3 pivotLocal = boat.InverseTransformPoint(handlePivot.position);
        Vector3 dir = handLocal - pivotLocal;

        float currentAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float deltaAngle = currentAngle - _initialGrabAngle;
        
        if (Mathf.Abs(deltaAngle) < 1f) return; // ignore tiny movements
        
        float targetRotation = _initialHandleRotation + deltaAngle;

        _currentYRotation = Mathf.MoveTowards(_currentYRotation,
            Mathf.Clamp(targetRotation, - maxRotation, maxRotation),
            rotationSpeed * Time.fixedDeltaTime);

        transform.rotation = Quaternion.Euler(0f, _currentYRotation, 0f) * boat.rotation;
        transform.position = boat.TransformPoint(_localOffset);

        _triggerValue = throttleAction.action.ReadValue<float>(); // 0 to 1
        OnThrottleChanged?.Invoke(_triggerValue); // Send value to the Boat script

        Vector3 buttonPositionTemp = _buttonPosition;
        buttonPositionTemp.y = _buttonPosition.y - (_triggerValue * maxButtonYMovement);
        throttleButtonVisual.localPosition = buttonPositionTemp;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        _grabbingHand = args.interactorObject.transform;

        Vector3 handLocal = boat.InverseTransformPoint(_grabbingHand.position);
        Vector3 pivotLocal = boat.InverseTransformPoint(handlePivot.position);
        Vector3 dir = handLocal - pivotLocal;

        _initialGrabAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        _initialHandleRotation = _currentYRotation;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        _grabbingHand = null;
    }
}