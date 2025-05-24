using System;
using Game;
using UnityEngine;
using UnityEngine.Serialization;

public class FishingArea : MonoBehaviour
{
    public Color _defaultColor = Color.green;
    public Color _enterColor = Color.red;
    public event Action<FishingArea> onDeath;
    
    [Header("Area Settings")]
    [Tooltip("What difficulty this fishing area is.")]
    [field: SerializeField]
    public FishingGame.Difficulty areaDifficulty { get; set; } = FishingGame.Difficulty.Easy;

    private void Start()
    {
        DebugFishingRodExit();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("FishingBait")) return;
        
        if (FishingGame.instance.fishingGameState != FishingGame.FishingGameState.NotStarted &&
            FishingGame.instance.fishingGameState != FishingGame.FishingGameState.WaitingFish) return;
        
        FishingGame.instance.difficulty = areaDifficulty;
        
        DebugFishingRodEnter();

        FishingGame.instance.StartGame();
        
        // Then used to make larry spawn where the last fish was fished
        // Ensures that it's on a valid water location and that it's near the player current position
        GameManager.instance.lastFishSpawnerPosition = transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("FishingBait")) return;
        
        if (FishingGame.instance.fishingGameState != FishingGame.FishingGameState.NotStarted &&
            FishingGame.instance.fishingGameState != FishingGame.FishingGameState.WaitingFish) return;
        
        DebugFishingRodExit();

        FishingGame.instance.ExitFishingArea();
    }

    public void BeforeDestroy()
    {
        // tell anyone who’s listening that we’re about to die
        onDeath?.Invoke(this);
    }
    
    private void DebugFishingRodExit()
    {
        GetComponent<Renderer>().material.color = _defaultColor;
    }
    
    private void DebugFishingRodEnter()
    {
        GetComponent<Renderer>().material.color = _enterColor;
    }
}
