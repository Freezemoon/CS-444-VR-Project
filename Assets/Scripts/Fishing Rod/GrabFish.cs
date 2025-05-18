using Game;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class GrabFish : MonoBehaviour
{
    public FishingGame.Difficulty difficulty;
    
    private XRGrabInteractable _grabInteractable;

    void Awake()
    {
        _grabInteractable = GetComponent<XRGrabInteractable>();
    }

    void OnEnable()
    {
        _grabInteractable.selectEntered.AddListener(OnGrab);
    }

    void OnDisable()
    {
        _grabInteractable.selectEntered.RemoveListener(OnGrab);
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        GameManager.instance.SetDialogueState(GameManager.DialogueState.DropFishInBucket);
        Destroy(GetComponent<ConfigurableJoint>());
        FishingGame.instance.ResetGameWhenFishIsGrabbedByUser();
    }
}
