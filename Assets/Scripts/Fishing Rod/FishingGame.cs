using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class FishingGame : MonoBehaviour
{
    public TMP_Text tmpText;

    public int minPull = 3;
    public int maxPull = 6;
    public float minWaitingFishTime = 2.5f;
    public float maxWaitingFishTime = 10;
    public int minPhaseBeforeWin = 3;
    public int maxPhaseBeforeWin = 8;
    
    public enum GameState
    {
        NotStarted,
        WaitingFish,
        Pulling,
        Reeling,
        Win
    }
    
    public GameState gameState { get; set; } = GameState.NotStarted;
    
    public static FishingGame instance { get; private set; }
    
    private int _currentPull;
    private int _neededPull;

    private float _neededWaitingFishTime;
    private float _currentWaitingFishTime;
    
    private int _neededPhaseBeforeWin;
    private int _currentPhaseBeforeWin;

    private void Awake()
    {
        // Check if instance already exists and destroy duplicates
        if (instance && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // Optional: persist across scenes
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        UpdateText();
    }

    private void Update()
    {
        switch (gameState)
        {
            case GameState.WaitingFish:
                _currentWaitingFishTime += Time.deltaTime;
                if (_currentWaitingFishTime >= _neededWaitingFishTime)
                {
                    StartPulling();
                }
                break;
        }
    }

    public void StartGame()
    {
        gameState = GameState.WaitingFish;

        _currentWaitingFishTime = 0;
        _neededWaitingFishTime = Random.Range(minWaitingFishTime, maxWaitingFishTime);

        _currentPhaseBeforeWin = 0;
        _neededPhaseBeforeWin = Random.Range(minPhaseBeforeWin, maxPhaseBeforeWin + 1);
        
        UpdateText();
    }
    
    public void LoseGame()
    {
        gameState = GameState.NotStarted;
        
        UpdateText();
    }
    

    public void PullSuccess()
    {
        _currentPull++;

        if (_currentPull >= _neededPull)
        {
            NextGamePhase();
        }
        
        UpdateText();
    }

    private void StartPulling()
    {
        gameState = GameState.Pulling;
        
        _currentPull = 0;
        _neededPull = Random.Range(minPull, maxPull + 1);
        
        UpdateText();
    }

    private void NextGamePhase()
    {
        _currentPhaseBeforeWin++;
        
        if (_currentPhaseBeforeWin >= _neededPhaseBeforeWin)
        {
            gameState = GameState.Win;
            return;
        }
        
        if (gameState == GameState.Pulling)
        {
            StartReeling();
        }
        else if (gameState == GameState.Reeling)
        {
            StartPulling();
        }
    }

    private void StartReeling()
    {
        gameState = GameState.Reeling;
        
        // TODO define how much to reel
        NextGamePhase(); // TEMP
        
        UpdateText();
    }

    private void UpdateText()
    {
        switch (gameState)
        {
            case GameState.NotStarted:
            case GameState.Reeling:
            case GameState.Win:
                tmpText.text = gameState.ToString() + ":";
                break;
            case GameState.WaitingFish:
                tmpText.text = gameState.ToString() + ": " + _currentWaitingFishTime + "/" + _neededWaitingFishTime;
                break;
            case GameState.Pulling:
                tmpText.text = gameState.ToString() + ": " + _currentPull + "/" + _neededPull;
                break;
        }
    }
}
