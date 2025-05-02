using UnityEngine;

public class Boat : MonoBehaviour
{
    public Transform leverTransform; // Reference to the handle
    public float speedMultiplier = 4f; // Multiplie the boat speed
    public float turnSensitivity = 0.2f; // Boat turn sensitivity
    private bool engine = false;
    private int reverse = 1; // 1 = forward, -1 = reverse
    private float speed = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        leverTransform.GetComponent<BoatHandle>().OnThrottleChanged += UpdateThrottle;
    }

    // Update is called once per frame
    void Update()
    {
        Quaternion boatToHandle = Quaternion.Inverse(transform.rotation) * leverTransform.rotation;
        float localAngle = boatToHandle.eulerAngles.y;
        if (localAngle > 180f) localAngle -= 360f;
        if (engine == true)
        {
            transform.position -= transform.right * speed * reverse * Time.deltaTime;

            if (speed > 0f)
            {
                float turnSpeed = localAngle * turnSensitivity; // How fast the boat turns
                transform.Rotate(0, - turnSpeed * Time.deltaTime, 0);
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
        else
        {
            Debug.Log("Something entered the boat trigger zone");
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
            reverse = 1; 
            Debug.Log("Engine is on and reverse is off"); 
        }
        else
        {
            engine = false;
            Debug.Log("Engine is off"); 
        }
    }

    public void EngineButtonReverse()
    {
        if (reverse == 1)
        {
            reverse = -1; // Turn on the reverse
            Debug.Log("Reverse is on");
        }
        else
        {
            reverse = 1;
            Debug.Log("Reverse is off"); 
        }
    }
}
