using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;

[RequireComponent(typeof(Collider))]


/// <summary>
/// Teleports the player to the destination. Used on the water to prevent the player to do down in it.
/// </summary>
public class TeleportPlayer : MonoBehaviour
{
    [Tooltip("A reference to the TeleportationProvider of the XR Origin")]
    [SerializeField] TeleportationProvider _teleportationProvider = null;

    [Tooltip("Destination of the player.")]
    [SerializeField] Transform _respawnAnchor = null;

    void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        
        var req = new TeleportRequest()
        {
            destinationPosition = _respawnAnchor.position,
            destinationRotation = _respawnAnchor.rotation
        };
        
        _teleportationProvider.QueueTeleportRequest(req);
    }
}