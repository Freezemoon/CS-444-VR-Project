using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class ShrinkOnGrab : MonoBehaviour
{
    public Vector3 grabbedScale = new Vector3(0.5f, 0.5f, 0.5f); // Target scale when grabbed
    private Vector3 _originalScale;

    private XRGrabInteractable _grabInteractable;

    private void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _originalScale = transform.localScale;

        _grabInteractable.selectEntered.AddListener(OnGrab);
        _grabInteractable.selectExited.AddListener(OnRelease);
    }

    private void OnDestroy()
    {
        _grabInteractable.selectEntered.RemoveListener(OnGrab);
        _grabInteractable.selectExited.RemoveListener(OnRelease);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        transform.localScale = grabbedScale;
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        // transform.localScale = _originalScale;
    }
}