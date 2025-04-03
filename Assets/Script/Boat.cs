using UnityEngine;

public class Boat : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(-0.2f, 0, 0) * Time.deltaTime; // Move the boat forward at a speed of 0.5 units per second
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
}
