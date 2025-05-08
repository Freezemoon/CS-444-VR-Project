using UnityEngine;

public class Boat : MonoBehaviour
{
    public Transform leverTransform; // Reference to the handle
    public float speedMultiplier = 3f; // Multiplie the boat speed
    public float turnSensitivity = 0.25f; // Boat turn sensitivity
    public float accelerationRate = 1.5f;
    private bool engine = false;
    private int reverse = 1; // 1 = forward, -1 = reverse
    private float speed = 0f;
    private float currentSpeed = 0f;

    public AudioClip engineStartClip;
    public AudioClip engineLoopClip;
    private AudioSource engineAudioSource;

    public GameObject OnOffIndicatorLamp;
    public GameObject reverseIndicatorLamp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
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
    void Update()
    {
        Quaternion boatToHandle = Quaternion.Inverse(transform.rotation) * leverTransform.rotation;
        float localAngle = boatToHandle.eulerAngles.y;
        if (localAngle > 180f) localAngle -= 360f;
        if (engine == true)
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, accelerationRate * Time.deltaTime);
            transform.position -= transform.right * currentSpeed * reverse * Time.deltaTime;

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
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(transform); // Set the boat as a parent of the player
            Debug.Log("Player entered the boat trigger zone");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.SetParent(null); // Unset the boat as a parent of the player
            Debug.Log("Player exited the boat trigger zone");
        }
    }

    public void EngineButtonOn()
    {
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
