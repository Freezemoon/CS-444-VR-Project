using System.Collections.Generic;
using Game;
using UnityEngine;
using UnityEngine.Serialization;

public class Boat : MonoBehaviour
{
    public Transform leverTransform; // Reference to the handle
    public Transform boatInitPos;
    public Transform boatLake2Pos;
    public float speedMultiplier = 3f; // Multiplie the boat speed
    public float turnSensitivity = 0.25f; // Boat turn sensitivity
    public float accelerationRate = 1.5f;

    public AudioClip engineStartClip;
    public AudioClip engineLoopClip;

    public GameObject onOffIndicatorLamp;
    public GameObject reverseIndicatorLamp;
    
    private AudioSource _engineAudioSource;
    private bool _engine;
    private int _reverse = 1; // 1 = forward, -1 = reverse
    private float _speed;
    private float _currentSpeed;
    
    // Fields for collision-based movement restriction
    private const float PushForce = 1.2f;
    private readonly List<Vector3> _activePushDirections = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        leverTransform.GetComponent<BoatHandle>().OnThrottleChanged += UpdateThrottle;

        onOffIndicatorLamp.GetComponent<MeshRenderer>().enabled = false;
        reverseIndicatorLamp.GetComponent<MeshRenderer>().enabled = false;

        _engineAudioSource = gameObject.AddComponent<AudioSource>();
        _engineAudioSource.loop = true;
        _engineAudioSource.playOnAwake = false;
        _engineAudioSource.clip = engineLoopClip;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        Quaternion boatToHandle = Quaternion.Inverse(transform.rotation) * leverTransform.rotation;
        float localAngle = boatToHandle.eulerAngles.y;
        if (localAngle > 180f) localAngle -= 360f;
        
        if (!_engine) return;
        
        _currentSpeed = Mathf.MoveTowards(_currentSpeed, _speed, accelerationRate * Time.fixedDeltaTime);
            
        Vector3 moveDirection = -transform.right * _reverse;
            
        if (_activePushDirections.Count > 0)
        {
            Vector3 totalPush = Vector3.zero;
            foreach (var dir in _activePushDirections)
            {
                totalPush += dir;
            }

            totalPush = totalPush.normalized;
            moveDirection += totalPush * PushForce;
            moveDirection = moveDirection.normalized;
        }

        transform.position += moveDirection * _currentSpeed * Time.fixedDeltaTime;
        
        if (_currentSpeed > 0f)
        {
            float turnSpeed = localAngle * turnSensitivity; // How fast the boat turns
            transform.Rotate(0, - turnSpeed * Time.fixedDeltaTime , 0);
        }
        if (_speed == 0f)
        {
            _engineAudioSource.volume = 0.7f;
            _engineAudioSource.pitch = 1f;
        }
        else if (_reverse == -1)
        {
            _engineAudioSource.volume = 1f;
            _engineAudioSource.pitch = 0.7f;
        }
        else
        {
            _engineAudioSource.volume = 1f;
            _engineAudioSource.pitch = 1.5f;
        }
    }

    private void UpdateThrottle(float value)
    {
        _speed = value * speedMultiplier; // Value goes from 0 to 1 based on trigger press
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LakeBorder"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 dir = (transform.position - contactPoint).normalized;
            _activePushDirections.Add(dir);
        }
        else if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform);
            Debug.Log("Player entered the boat trigger zone");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("LakeBorder"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 dir = (transform.position - contactPoint).normalized;

            // Remove approximate match from list
            _activePushDirections.RemoveAll(d => Vector3.Angle(d, dir) < 10f); // Angle threshold to remove correct one
        }
        else if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null);
            Debug.Log("Player exited the boat trigger zone");
        }
    }
    
    public void EngineButtonOn()
    {
        GameManager.instance.SetDialogueState(GameManager.DialogueState.AccelerateBoat);
        if (_engine == false)
        {
            _engine = true; // Turn on the engine
            onOffIndicatorLamp.GetComponent<MeshRenderer>().enabled = true;

            AudioSource.PlayClipAtPoint(engineStartClip, transform.position);
            _engineAudioSource.Play();
            Debug.Log("Engine is on"); 
        }
        else
        {
            _engine = false;
            onOffIndicatorLamp.GetComponent<MeshRenderer>().enabled = false;

            _engineAudioSource.Stop();
            Debug.Log("Engine is off"); 
        }
    }

    public void EngineButtonReverse()
    {
        if (_reverse == 1)
        {
            _reverse = -1; // Turn on the reverse
            reverseIndicatorLamp.GetComponent<MeshRenderer>().enabled = true;
            Debug.Log("Reverse is on");
        }
        else
        {
            _reverse = 1;
            reverseIndicatorLamp.GetComponent<MeshRenderer>().enabled = false;
            Debug.Log("Reverse is off"); 
        }
    }

    public void SpawnBackToInitPosBoat()
    {
        transform.position = boatInitPos.position;
        transform.rotation = boatInitPos.rotation;
    }
    
    public void SpawnBackToLake2Pos()
    {
        transform.position = boatLake2Pos.position;
        transform.rotation = boatLake2Pos.rotation;
    }
}
