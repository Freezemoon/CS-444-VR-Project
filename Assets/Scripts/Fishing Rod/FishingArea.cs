using System;
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
        FishingRodExit();
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("FishingBait")) return;
        
        if (FishingGame.instance.gameState != FishingGame.GameState.NotStarted &&
            FishingGame.instance.gameState != FishingGame.GameState.WaitingFish) return;
        
        FishingGame.instance.difficulty = areaDifficulty;
        
        FishingRodEnter();

        FishingGame.instance.StartGame();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("FishingBait")) return;
        
        if (FishingGame.instance.gameState != FishingGame.GameState.NotStarted &&
            FishingGame.instance.gameState != FishingGame.GameState.WaitingFish) return;
        
        FishingRodExit();

        FishingGame.instance.ExitFishingArea();
    }

    public void BeforeDestroy()
    {
        // tell anyone who’s listening that we’re about to die
        onDeath?.Invoke(this);
    }
    
    private void FishingRodExit()
    {
        GetComponent<Renderer>().material.color = _defaultColor;
    }
    
    private void FishingRodEnter()
    {
        GetComponent<Renderer>().material.color = _enterColor;
    }
}
