using System;
using UnityEngine;

public class BaitManager : MonoBehaviour
{
    private Rigidbody _rb = null;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("FishingZone"))
        {
            if (FishingGame.instance.canBaitGoInWater &&
                FishingGame.instance.gameState == FishingGame.GameState.WaitingFish)
            {
                Destroy(other.gameObject);
            }
        }
        
        if (!other.CompareTag("WaterFishingLimit")) return;
        
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.linearDamping = 100f;
        _rb.angularDamping = 100f;

        float offset = 0;
        if (FishingGame.instance.canBaitGoInWater &&
            FishingGame.instance.gameState == FishingGame.GameState.WaitingFish)
        {
            offset = -0.05f;
        }
        
        Vector3 corrected = new Vector3(_rb.transform.position.x,
            other.bounds.max.y + offset, _rb.transform.position.z);
        
        _rb.MovePosition(corrected);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("WaterFishingLimit")) return;
        
        _rb.useGravity = true;
        _rb.linearDamping = 0;
        _rb.angularDamping = 0.05f;
    }
}
