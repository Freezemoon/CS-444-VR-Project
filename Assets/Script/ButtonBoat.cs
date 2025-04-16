using UnityEngine;
using UnityEngine.Events;

public class ButtonBoat : MonoBehaviour
{
    bool isPressed;
    public GameObject Button; // Reference to the button object
    public UnityEvent onPress;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPressed = false; // Initialize the button state to not pressed
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isPressed)
        {
            Button.transform.localPosition -= new Vector3(0, 0.003f, 0);
            isPressed = true; // Set the button state to pressed
            onPress.Invoke(); // Invoke the UnityEvent when the button is pressed
            Debug.Log("Button pressed");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (isPressed)
        {
            Button.transform.localPosition += new Vector3(0, 0.003f, 0);
            isPressed = false; // Set the button state to not pressed
            Debug.Log("Button released");
        }
    }
}
