using System;
using UnityEngine;

public class FishingArea : MonoBehaviour
{
    public Color _defaultColor = Color.green;
    public Color _enterColor = Color.red;

    private void Start()
    {
        FishingRodExit();
    }
    
    private void FishingRodExit()
    {
        GetComponent<Renderer>().material.color = _defaultColor;
    }
    
    private void FishingRodEnter()
    {
        GetComponent<Renderer>().material.color = _enterColor;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("FishingBait")) return;
        
        FishingRodEnter();

        FishingGame.instance.StartGame();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("FishingBait")) return;
        
        FishingRodExit();

        FishingGame.instance.ExitFishingArea();
    }
}
