using UnityEngine;
using UnityEngine.XR;

public class Boat : MonoBehaviour
{
    public XRNode inputSource = XRNode.RightHand;
    public HingeJoint hinge;
    public float maxSpeed = 2f; // Max boat speed
    public float turnSensitivity = 0.2f; // Boat turn sensitivity

    private bool engine = false;
    private int reverse = 1; // 1 = forward, -1 = reverse
    private InputDevice device;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Get the InputDevice if not valid
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(inputSource);
        }

        float triggerValue = 0f;
        if (device.TryGetFeatureValue(CommonUsages.trigger, out float value))
        {
            triggerValue = value;
        }

        float angle = hinge.angle;
        if (float.IsNaN(angle)) angle = 0;
        if (engine == true)
        {
            float speed = 0.5f * maxSpeed; //0.5f replaces the triggervalue because it don't work
            transform.position -= transform.right * speed * reverse * Time.deltaTime;

            float turnSpeed = angle * turnSensitivity; // How fast to turn
            transform.Rotate(0, turnSpeed * Time.deltaTime, 0);
        }
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
            Debug.Log("Something entered the boat trigger zone"); // Debug message
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
