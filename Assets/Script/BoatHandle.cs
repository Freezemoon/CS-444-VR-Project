using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class BoatHandle : MonoBehaviour
{
    [Header("References")]
    public Transform boat;
    public Transform handlePivot;

    [Header("Settings")]
    public float maxRotation = 45f;
    public float rotationSensitivity = 300f; // Increase for snappier response

    private Transform grabbingHandTransform;
    private float currentYRotation;
    private Vector3 localOffset;

    private float initialHandX;
    private float startRotation;

    private void Start()
    {
        localOffset = boat.InverseTransformPoint(transform.position);
        currentYRotation = transform.localEulerAngles.y;
        if (currentYRotation > 180f) currentYRotation -= 360f;
    }

    private void Update()
    {
        FollowBoat();

        if (grabbingHandTransform != null)
        {
            float handDelta = grabbingHandTransform.position.x - initialHandX;
            float deltaY = handDelta * rotationSensitivity;
            currentYRotation = Mathf.Clamp(startRotation + deltaY, -maxRotation, maxRotation);

            transform.localRotation = Quaternion.Euler(0f, currentYRotation, 0f);
        }
    }

    private void FollowBoat()
    {
        transform.position = boat.TransformPoint(localOffset);
        transform.rotation = boat.rotation * Quaternion.Euler(0f, currentYRotation, 0f);
    }

    public void OnGrab(SelectEnterEventArgs args)
    {
        grabbingHandTransform = args.interactorObject.transform;
        startRotation = currentYRotation;

        // Cache the hand's world X position
        initialHandX = grabbingHandTransform.position.x;
    }

    public void OnRelease(SelectExitEventArgs args)
    {
        grabbingHandTransform = null;
    }
}
