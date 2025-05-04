using System;
using UnityEngine;

public class BaitManager : MonoBehaviour
{
    private Rigidbody _rb = null;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("WaterFishingLimit")) return;
        
        _rb.useGravity = false;
        _rb.linearVelocity = Vector3.zero;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("WaterFishingLimit")) return;
        
        _rb.useGravity = true;
    }
}
