using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class BoatHandle : MonoBehaviour
{
    [Header("References")]
    public Transform boat;
    public Transform handlePivot;
    public XRGrabInteractable grabInteractable;

    [Header("Settings")]
    public float maxRotation = 45f;
    public float rotationSpeed = 150f;

    private Transform _grabbingHand;
    private float _currentYRotation;
    private Vector3 _localOffset;
    private float _initialGrabAngle;
    private float _initialHandleRotation;

    private void Start()
    {
        _localOffset = boat.InverseTransformPoint(transform.position);

        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void Update()
    {
        if (_grabbingHand == null)
        {
            transform.position = boat.TransformPoint(_localOffset);
            transform.rotation = boat.rotation * Quaternion.Euler(0f, _currentYRotation, 0f);
        }
        else
        {
            Vector3 handLocal = boat.InverseTransformPoint(_grabbingHand.position);
            Vector3 pivotLocal = boat.InverseTransformPoint(handlePivot.position);
            Vector3 dir = handLocal - pivotLocal;

            float currentAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float deltaAngle = currentAngle - _initialGrabAngle;

            float targetRotation = _initialHandleRotation + deltaAngle;
            targetRotation = Mathf.Clamp(targetRotation, -maxRotation, maxRotation);

            _currentYRotation = Mathf.MoveTowards(_currentYRotation, targetRotation, rotationSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Euler(0f, _currentYRotation, 0f) * boat.rotation;
            transform.position = boat.TransformPoint(_localOffset);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        _grabbingHand = args.interactorObject.transform;

        // Calculate initial angle between hand and pivot (in boat local space)
        Vector3 handLocal = boat.InverseTransformPoint(_grabbingHand.position);
        Vector3 pivotLocal = boat.InverseTransformPoint(handlePivot.position);
        Vector3 dir = handLocal - pivotLocal;

        _initialGrabAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        _initialHandleRotation = _currentYRotation;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        _grabbingHand = null;

        // Clamp rotation within bounds after release
        _currentYRotation = Mathf.Clamp(_currentYRotation, -maxRotation, maxRotation);
    }

    public float GetNormalizedSteerAmount()
    {
        // Returns -1 to 1 based on handle rotation
        return _currentYRotation / maxRotation;
    }
}