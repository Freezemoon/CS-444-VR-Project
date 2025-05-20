using Game;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GrabFish : MonoBehaviour
{
    public FishingGame.Difficulty difficulty;
    
    private XRGrabInteractable _grabInteractable;
    private Rigidbody _rb;

    void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        _grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void OnDisable()
    {
        _grabInteractable.selectEntered.RemoveListener(OnGrab);
    }
    
    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("WaterFishingLimit")) return;
        if (!GetComponent<ConfigurableJoint>()) return;
        
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.linearDamping = 100f;
        _rb.angularDamping = 100f;
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("WaterFishingLimit")) return;
        if (!GetComponent<ConfigurableJoint>()) return;
        
        _rb.useGravity = true;
        _rb.linearDamping = 0;
        _rb.angularDamping = 0.05f;
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        GameManager.instance.SetDialogueState(GameManager.DialogueState.DropFishInBucket);
        Destroy(GetComponent<ConfigurableJoint>());
        FishingGame.instance.ResetGameWhenFishIsGrabbedByUser();
        
        _rb.useGravity = true;
        _rb.linearDamping = 0;
        _rb.angularDamping = 0.05f;
    }
}
