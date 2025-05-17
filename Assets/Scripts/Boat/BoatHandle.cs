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

    private Transform grabbingHand;
    private float currentYRotation;
    private Vector3 localOffset;
    private float triggerValue;
    private float initialGrabAngle;
    private float initialHandleRotation;
    private Vector3 buttonPosition;

    private void Start()
    {
        localOffset = boat.InverseTransformPoint(transform.position);
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
        throttleAction.action.Enable();

        buttonPosition = throttleButtonVisual.localPosition;
    }

    private void OnDestroy()
    {
        grabInteractable.selectEntered.RemoveListener(OnGrab);
        grabInteractable.selectExited.RemoveListener(OnRelease);
        throttleAction.action.Disable();
    }

    private void Update()
    {
        if (grabbingHand == null)
        {
            transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f) * boat.rotation;
            transform.position = boat.TransformPoint(localOffset);

            triggerValue = 0f;
            OnThrottleChanged?.Invoke(triggerValue);

            throttleButtonVisual.localPosition = buttonPosition;
            return;
        }

        Vector3 handLocal = boat.InverseTransformPoint(grabbingHand.position);
        Vector3 pivotLocal = boat.InverseTransformPoint(handlePivot.position);
        Vector3 dir = handLocal - pivotLocal;

        float currentAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        float deltaAngle = currentAngle - initialGrabAngle;
        float targetRotation = initialHandleRotation + deltaAngle;

        currentYRotation = Mathf.MoveTowards(currentYRotation, Mathf.Clamp(targetRotation, - maxRotation, maxRotation), rotationSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, currentYRotation, 0f) * boat.rotation;
        transform.position = boat.TransformPoint(localOffset);

        triggerValue = throttleAction.action.ReadValue<float>(); // 0 to 1
        OnThrottleChanged?.Invoke(triggerValue); // Send value to the Boat script

        Vector3 buttonPositionTemp = buttonPosition;
        buttonPositionTemp.y = buttonPosition.y - (triggerValue * maxButtonYMovement);
        throttleButtonVisual.localPosition = buttonPositionTemp;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        grabbingHand = args.interactorObject.transform;

        Vector3 handLocal = boat.InverseTransformPoint(grabbingHand.position);
        Vector3 pivotLocal = boat.InverseTransformPoint(handlePivot.position);
        Vector3 dir = handLocal - pivotLocal;

        initialGrabAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        initialHandleRotation = currentYRotation;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        grabbingHand = null;
    }
}