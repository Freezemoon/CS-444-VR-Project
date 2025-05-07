using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using Random = UnityEngine.Random;

public class FishingGame : MonoBehaviour
{
    public TMP_Text tmpText;
    
    [FormerlySerializedAs("audioSource")] public AudioSource phaseSuccessAudioSource;

    [Header("Haptics")]
    public HapticImpulsePlayer rightHandHaptics;
    public HapticImpulsePlayer leftHandHaptics;

    [Header("Parameters")]
    public int minPull = 3;
    public int maxPull = 6;
    public float minWaitingFishTime = 2.5f;
    public float maxWaitingFishTime = 10;
    public float maxPullingTimeBeforeLose = 10;
    public float maxReelingTimeBeforeLose = 10;
    public float minReelLength = 1.5f;
    public float maxReelLength = 3;
    public int minPhaseBeforeWin = 3;
    public int maxPhaseBeforeWin = 7;
    public float reelForceMultiplierDivisor = 10;
    
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

    private float _neededPhaseTimeBeforeLose;
    private float _currentPhaseTimeBeforeLose;
    
    private int _neededPhaseBeforeWin;
    private int _currentPhaseBeforeWin;

    private float _neededReel;
    private float _currentReel;

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
                    break;
                }

                UpdateText();
                
                break;
            case GameState.Pulling:
            case GameState.Reeling:
                _currentPhaseTimeBeforeLose += Time.deltaTime;
                
                if (_currentPhaseTimeBeforeLose >= _neededPhaseTimeBeforeLose)
                {
                    LoseGame();
                }

                UpdateText();

                break;
            case GameState.NotStarted:
            case GameState.Win:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void FishGrabbed()
    {
        gameState = GameState.NotStarted;
    }

    public void StartGame()
    {
        gameState = GameState.WaitingFish;

        _currentWaitingFishTime = 0;
        _neededWaitingFishTime = Random.Range(minWaitingFishTime, maxWaitingFishTime);

        _currentPhaseBeforeWin = 0;
        _neededPhaseBeforeWin = Random.Range(minPhaseBeforeWin, maxPhaseBeforeWin + 1);
    }

    public void ExitFishingArea()
    {
        if (gameState == GameState.WaitingFish)
        {
            LoseGame();
        }
    }
    
    public void LoseGame()
    {
        if (gameState == GameState.Win) return;
        
        gameState = GameState.NotStarted;
        
        UpdateText();
    }

    public bool ReelSuccess(float amount)
    {
        if (gameState != GameState.Reeling) return false;
        
        _currentReel += amount;

        if (!(_currentReel >= _neededReel)) return false;
        
        phaseSuccessAudioSource?.Play();
        NextGamePhase();
        
        return true;
    }
    

    public void PullSuccess()
    {
        if (gameState != GameState.Pulling) return;
        
        _currentPull++;

        if (_currentPull < _neededPull) return;
        
        phaseSuccessAudioSource?.Play();
        NextGamePhase();
    }

    private void StartPulling()
    {
        gameState = GameState.Pulling;
        rightHandHaptics.SendHapticImpulse(0.8f, 0.8f);
        
        _currentPull = 0;
        _neededPull = Random.Range(minPull, maxPull + 1);
        
        _currentPhaseTimeBeforeLose = 0;
        _neededPhaseTimeBeforeLose = maxPullingTimeBeforeLose;
    }

    private void StartReeling()
    {
        gameState = GameState.Reeling;
        leftHandHaptics.SendHapticImpulse(0.8f, 0.8f);
        
        _currentReel = 0;
        _neededReel = Random.Range(minReelLength, maxReelLength);
        
        _currentPhaseTimeBeforeLose = 0;
        _neededPhaseTimeBeforeLose = maxReelingTimeBeforeLose;
    }

    private void NextGamePhase()
    {
        _currentPhaseBeforeWin++;
        
        if (_currentPhaseBeforeWin >= _neededPhaseBeforeWin)
        {
            gameState = GameState.Win;
            
            phaseSuccessAudioSource?.Play();
            UpdateText();
            
            return;
        }
        
        switch (gameState)
        {
            case GameState.Pulling:
                StartReeling();
                break;
            case GameState.Reeling:
                StartPulling();
                break;
            case GameState.NotStarted:
            case GameState.WaitingFish:
            case GameState.Win:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UpdateText()
    {
        switch (gameState)
        {
            case GameState.NotStarted:
            case GameState.Win:
                tmpText.text = gameState.ToString() + ":";
                break;
            case GameState.WaitingFish:
                tmpText.text = gameState.ToString() + ": " + _currentWaitingFishTime.ToString("F2") + "/"
                               + _neededWaitingFishTime.ToString("F2");
                break;
            case GameState.Pulling:
                tmpText.text = gameState.ToString() + ": " + _currentPull + "/" + _neededPull + "\n"
                               + _currentPhaseTimeBeforeLose.ToString("F2") + "/"
                               + _neededPhaseTimeBeforeLose.ToString("F2") + "\n"
                               + _currentPhaseBeforeWin + "/" + _neededPhaseBeforeWin;
                break;
            case GameState.Reeling:
                tmpText.text = gameState.ToString() + ": " + _currentReel.ToString("F2") + "/" 
                               + _neededReel.ToString("F2") + "\n"
                               + _currentPhaseTimeBeforeLose.ToString("F2") + "/" 
                               + _neededPhaseTimeBeforeLose.ToString("F2") + "\n"
                               + _currentPhaseBeforeWin + "/" + _neededPhaseBeforeWin;
                break;
        }
    }
}
