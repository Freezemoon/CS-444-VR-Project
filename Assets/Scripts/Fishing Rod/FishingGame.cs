using System;
using Game;
using TMPro;
using UnityEngine;
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
    
    public GameObject easyFishPrefab;
    public GameObject mediumFishPrefab;
    public GameObject hardFishPrefab;
    public Transform baitFishAttach;

    [Header("Haptics")]
    public HapticImpulsePlayer rightHandHaptics;
    public HapticImpulsePlayer leftHandHaptics;
    
    [Header("Game Parameters")]
    public float maxPullingTimeBeforeLose = 12;
    public float maxReelingTimeBeforeLose = 15;
    public float reelForceMultiplierDivisor = 10;
    
    [Header("Easy")]
    public int minPullEasy = 2;
    public int maxPullEasy = 3;
    public float minWaitingFishTimeEasy = 5;
    public float maxWaitingFishTimeEasy = 8;
    public float minReelLengthEasy = 0.3f;
    public float maxReelLengthEasy = 0.5f;
    public int minPhaseBeforeWinEasy = 3;
    public int maxPhaseBeforeWinEasy = 4;
    
    [Header("Medium")]
    public int minPullMedium = 2;
    public int maxPullMedium = 4;
    public float minWaitingFishTimeMedium = 7;
    public float maxWaitingFishTimeMedium = 10;
    public float minReelLengthMedium = 0.4f;
    public float maxReelLengthMedium = 0.8f;
    public int minPhaseBeforeWinMedium = 2;
    public int maxPhaseBeforeWinMedium = 5;
    
    [Header("Hard")]
    public int minPullHard = 4;
    public int maxPullHard = 6;
    public float minWaitingFishTimeHard = 9;
    public float maxWaitingFishTimeHard = 12;
    public float minReelLengthHard = 0.8f;
    public float maxReelLengthHard = 1.5f;
    public int minPhaseBeforeWinHard = 4;
    public int maxPhaseBeforeWinHard = 6;
    
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
                    rightHandHaptics.SendHapticImpulse(0.8f, 2);
                    
                    GameObject fishPrefab = null;
                    switch (difficulty)
                    {
                        case Difficulty.Easy:
                            fishPrefab = easyFishPrefab;
                            break;
                        case Difficulty.Medium:
                            fishPrefab = mediumFishPrefab;
                            break;
                        case Difficulty.Hard:
                            fishPrefab = hardFishPrefab;
                            break;
                    }
                    _currentFish = Instantiate(fishPrefab, baitFishAttach.position, Quaternion.identity);
                    _currentFish.GetComponent<XRGrabInteractable>().enabled = false;
                    
                    ConfigurableJoint currentFishJoin = _currentFish.AddComponent<ConfigurableJoint>();
                    currentFishJoin.autoConfigureConnectedAnchor = false;
                    currentFishJoin.anchor = Vector3.zero;
                    currentFishJoin.xMotion = ConfigurableJointMotion.Locked;
                    currentFishJoin.yMotion = ConfigurableJointMotion.Locked;
                    currentFishJoin.zMotion = ConfigurableJointMotion.Locked;
                    
                    currentFishJoin.connectedBody = baitFishAttach.parent.GetComponent<Rigidbody>();
                    currentFishJoin.anchor = Vector3.left * 0.3f;
                    currentFishJoin.connectedAnchor = new Vector3(0, 0, 0);
                    
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
        
        rightHandHaptics.SendHapticImpulse(0.8f, 1.6f);
        
        _currentWaitingFishTime = 0;
        
        float minWaitingFishTime = 0;
        float maxWaitingFishTime = 0;
        switch (difficulty)
        {
            case Difficulty.Easy:
                minWaitingFishTime = minWaitingFishTimeEasy;
                maxWaitingFishTime = maxWaitingFishTimeEasy;
                break;
            case Difficulty.Medium:
                minWaitingFishTime = minWaitingFishTimeMedium;
                maxWaitingFishTime = maxWaitingFishTimeMedium;
                break;
            case Difficulty.Hard:
                minWaitingFishTime = minWaitingFishTimeHard;
                maxWaitingFishTime = maxWaitingFishTimeHard;
                break;
        }
        _neededWaitingFishTime = Random.Range(minWaitingFishTime, maxWaitingFishTime);
        
        _currentPhaseBeforeWin = 0;

        int minPhaseBeforeWin = 0;
        int maxPhaseBeforeWin = 0;
        switch (difficulty)
        {
            case Difficulty.Easy:
                minPhaseBeforeWin = minPhaseBeforeWinEasy;
                maxPhaseBeforeWin = maxPhaseBeforeWinEasy;
                break;
            case Difficulty.Medium:
                minPhaseBeforeWin = minPhaseBeforeWinMedium;
                maxPhaseBeforeWin = maxPhaseBeforeWinMedium;
                break;
            case Difficulty.Hard:
                minPhaseBeforeWin = minPhaseBeforeWinHard;
                maxPhaseBeforeWin = maxPhaseBeforeWinHard;
                break;
        }
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
        GameManager.instance.RestartFishingTutoIfLostBeforeGrabFish();
        
        UpdateText();
    }

    public bool ReelSuccess(float amount)
    {
        if (gameState != GameState.Reeling) return false;
        
        _currentReel += amount;

        if (_currentReel < _neededReel) return false;
        
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
        
        int minPull = 0;
        int maxPull = 0;
        switch (difficulty)
        {
            case Difficulty.Easy:
                minPull = minPullEasy;
                maxPull = maxPullEasy;
                break;
            case Difficulty.Medium:
                minPull = minPullMedium;    
                maxPull = maxPullMedium;
                break;
            case Difficulty.Hard:
                minPull = minPullHard;
                maxPull = maxPullHard;
                break;
        }
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
        
        float minReelLength = 0;
        float maxReelLength = 0;
        switch (difficulty)
        {
            case Difficulty.Easy:
                minReelLength = minReelLengthEasy;
                maxReelLength = maxReelLengthEasy;
                break;
            case Difficulty.Medium:
                minReelLength = minReelLengthMedium;
                maxReelLength = maxReelLengthMedium;
                break;
            case Difficulty.Hard:
                minReelLength = minReelLengthHard;
                maxReelLength = maxReelLengthHard;
                break;
        }
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
            Rigidbody _rb = _currentFish.GetComponent<Rigidbody>();
            _rb.useGravity = true;
            _rb.linearDamping = 0;
            _rb.angularDamping = 0.05f;
            
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
