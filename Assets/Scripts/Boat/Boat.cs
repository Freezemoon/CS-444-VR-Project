using System.Collections.Generic;
using Game;
using UnityEngine;

public class Boat : MonoBehaviour
{
    public Transform leverTransform; // Reference to the handle
    public float speedMultiplier = 3f; // Multiplie the boat speed
    public float turnSensitivity = 0.25f; // Boat turn sensitivity
    public float accelerationRate = 1.5f;

    public AudioClip engineStartClip;
    public AudioClip engineLoopClip;
    private AudioSource engineAudioSource;

    public GameObject OnOffIndicatorLamp;
    public GameObject reverseIndicatorLamp;
    
    private bool engine = false;
    private int reverse = 1; // 1 = forward, -1 = reverse
    private float speed = 0f;
    private float currentSpeed = 0f;
    
    // Fields for collision-based movement restriction
    private float pushForce = 1.2f;
    private List<Vector3> activePushDirections = new List<Vector3>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        leverTransform.GetComponent<BoatHandle>().OnThrottleChanged += UpdateThrottle;

        OnOffIndicatorLamp.GetComponent<MeshRenderer>().enabled = false;
        reverseIndicatorLamp.GetComponent<MeshRenderer>().enabled = false;

        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.loop = true;
        engineAudioSource.playOnAwake = false;
        engineAudioSource.clip = engineLoopClip;
    }

    // Update is called once per frame
    private void Update()
    {
        Quaternion boatToHandle = Quaternion.Inverse(transform.rotation) * leverTransform.rotation;
        float localAngle = boatToHandle.eulerAngles.y;
        if (localAngle > 180f) localAngle -= 360f;
        if (engine == true)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, accelerationRate * Time.deltaTime);
            
            Vector3 moveDirection = -transform.right * reverse;
            
            if (activePushDirections.Count > 0)
            {
                Vector3 totalPush = Vector3.zero;
                foreach (var dir in activePushDirections)
                {
                    totalPush += dir;
                }

                totalPush = totalPush.normalized;
                moveDirection += totalPush * pushForce;
                moveDirection = moveDirection.normalized;
            }

            transform.position += moveDirection * currentSpeed * Time.deltaTime;

            if (currentSpeed > 0f)
            {
                float turnSpeed = localAngle * turnSensitivity; // How fast the boat turns
                transform.Rotate(0, - turnSpeed * Time.deltaTime, 0);
            }
            if (speed == 0f)
            {
                engineAudioSource.volume = 0.7f;
                engineAudioSource.pitch = 1f;
            }
            else if (reverse == -1)
            {
                engineAudioSource.volume = 1f;
                engineAudioSource.pitch = 0.7f;
            }
            else
            {
                engineAudioSource.volume = 1f;
                engineAudioSource.pitch = 1.5f;
            }
        }
    }

    private void UpdateThrottle(float value)
    {
        speed = value * speedMultiplier; // Value goes from 0 to 1 based on trigger press
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("LakeBorder"))
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            Vector3 dir = (transform.position - contactPoint).normalized;
            activePushDirections.Add(dir);
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
            activePushDirections.RemoveAll(d => Vector3.Angle(d, dir) < 10f); // Angle threshold to remove correct one
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
        if (engine == false)
        {
            engine = true; // Turn on the engine
            OnOffIndicatorLamp.GetComponent<MeshRenderer>().enabled = true;

            AudioSource.PlayClipAtPoint(engineStartClip, transform.position);
            engineAudioSource.Play();
            Debug.Log("Engine is on and reverse is off"); 
        }
        else
        {
            engine = false;
            OnOffIndicatorLamp.GetComponent<MeshRenderer>().enabled = false;

            engineAudioSource.Stop();
            Debug.Log("Engine is off"); 
        }
    }

    public void EngineButtonReverse()
    {
        if (reverse == 1)
        {
            reverse = -1; // Turn on the reverse
            reverseIndicatorLamp.GetComponent<MeshRenderer>().enabled = true;
            Debug.Log("Reverse is on");
        }
        else
        {
            reverse = 1;
            reverseIndicatorLamp.GetComponent<MeshRenderer>().enabled = false;
            Debug.Log("Reverse is off"); 
        }
    }
}
