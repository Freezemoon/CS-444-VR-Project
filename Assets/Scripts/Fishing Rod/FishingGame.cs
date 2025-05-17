using System;
using System.Collections.Generic;
using Game;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Random = UnityEngine.Random;

public class FishingGame : MonoBehaviour
{
    public TMP_Text tmpText;
    
    public AudioSource phaseSuccessAudioSource;
    public AudioSource baitGoesInWaterAudioSource;
    public AudioSource loseAudioSource;
    public AudioSource victoryAudioSource;
    
    public GameObject fishPrefab;
    public Transform baitFishAttach;

    [Header("Haptics")]
    public HapticImpulsePlayer rightHandHaptics;
    public HapticImpulsePlayer leftHandHaptics;
    // public List<int> minPullList = new List<int>();
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

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }
    
    public GameState gameState { get; set; } = GameState.NotStarted;
    public Difficulty difficulty { get; set; } = Difficulty.Easy;
    public bool canStart { get; set; } = false;
    
    public bool canBaitGoInWater => gameState == GameState.WaitingFish && _currentWaitingFishTime >= _neededTimeBeforeBaitGoesInWater;
    
    public bool canGrabFish => gameState == GameState.Win && _currentFish;
    
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
    
    private readonly float _neededTimeBeforeBaitGoesInWater = 1.5f;
    
    private GameObject _currentFish;

    private bool _wasBaitAlreadyInWaterThisRound;

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

                if (!_wasBaitAlreadyInWaterThisRound &&
                    _currentWaitingFishTime >= _neededTimeBeforeBaitGoesInWater)
                {
                    GameManager.instance.SetDialogueState(GameManager.DialogueState.WaitingFish);
                    
                    _wasBaitAlreadyInWaterThisRound = true;
                    baitGoesInWaterAudioSource.Play();
                }
                
                // Check if fish can spawn
                if (_currentWaitingFishTime >= _neededWaitingFishTime)
                {
                    difficulty = (Difficulty)Enum.GetValues(typeof(Difficulty)).GetValue(
                        Random.Range(0, Enum.GetValues(typeof(Difficulty)).Length));
                    
                    _currentFish = Instantiate(fishPrefab, baitFishAttach.position, Quaternion.identity);
                    _currentFish.GetComponent<XRGrabInteractable>().enabled = false;
                    _currentFish.GetComponent<Rigidbody>().useGravity = false;
                    Physics.IgnoreCollision(baitFishAttach.parent.GetComponent<Collider>(), _currentFish.GetComponent<Collider>());
                    ConfigurableJoint currentFishJoin = _currentFish.GetComponent<ConfigurableJoint>();
                    currentFishJoin.connectedBody = baitFishAttach.parent.GetComponent<Rigidbody>();
                    currentFishJoin.anchor = Vector3.left * 1.1f;
                    currentFishJoin.connectedAnchor = new Vector3(0, 0, 0);
                    
                    _neededWaitingFishTime = Random.Range(minWaitingFishTime, maxWaitingFishTime);
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

    public void ResetGameWhenFishIsGrabbedByUser()
    {
        gameState = GameState.NotStarted;
    }

    public void StartGame()
    {
        if (!canStart) return;

        _wasBaitAlreadyInWaterThisRound = false;
        gameState = GameState.WaitingFish;
        
        rightHandHaptics.SendHapticImpulse(0.8f, 0.5f);

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
        
        Destroy(_currentFish);
        _currentFish = null;
        
        loseAudioSource.Play();
        GameManager.instance.restartFishingTutoIfLostBeforeGrabFish();
        
        UpdateText();
    }

    public bool ReelSuccess(float amount)
    {
        if (gameState != GameState.Reeling) return false;
        
        _currentReel += amount;

        if (!(_currentReel >= _neededReel)) return false;
        
        NextGamePhase();
        
        return true;
    }
    

    public void PullSuccess()
    {
        if (gameState != GameState.Pulling) return;
        
        _currentPull++;

        if (_currentPull < _neededPull) return;
        
        NextGamePhase();
    }

    private void StartPulling()
    {
        GameManager.instance.SetDialogueState(GameManager.DialogueState.PullFight);
        
        gameState = GameState.Pulling;
        rightHandHaptics.SendHapticImpulse(0.8f, 0.8f);
        
        _currentPull = 0;
        _neededPull = Random.Range(minPull, maxPull + 1);
        
        _currentPhaseTimeBeforeLose = 0;
        _neededPhaseTimeBeforeLose = maxPullingTimeBeforeLose;
    }

    private void StartReeling()
    {
        GameManager.instance.SetDialogueState(GameManager.DialogueState.ReelFight);
        
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
        
        // Check if win
        if (_currentPhaseBeforeWin >= _neededPhaseBeforeWin)
        {
            GameManager.instance.SetDialogueState(GameManager.DialogueState.GrabFish);
            
            gameState = GameState.Win;
            
            _currentFish.GetComponent<XRGrabInteractable>().enabled = true;
            _currentFish.GetComponent<Rigidbody>().useGravity = true;
            
            victoryAudioSource?.Play();
            UpdateText();
            
            return;
        }
        
        phaseSuccessAudioSource?.Play();
        
        switch (gameState)
        {
            case GameState.Pulling:
                StartReeling();
                break;
            case GameState.Reeling:
                GameManager.instance.SetDialogueState(GameManager.DialogueState.AlternatePullReel);
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
