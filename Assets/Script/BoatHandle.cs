using UnityEngine;

public class LimitedRotation : MonoBehaviour
{
    public float minRotation = -45f; // Minimum angle
    public float maxRotation = 45f;  // Maximum angle
    public string axis = "y"; // Axis of rotation

    void Update()
    {
        Vector3 rotation = transform.localEulerAngles;
        float angle = rotation.z;

        if (axis == "x") angle = rotation.x;
        else if (axis == "y") angle = rotation.y;

        if (angle > 180) angle -= 360; // Convert to -180 to 180 range

        // Clamp rotation
        angle = Mathf.Clamp(angle, minRotation, maxRotation);

        if (axis == "x") rotation.x = angle;
        else if (axis == "y") rotation.y = angle;
        else rotation.z = angle;

        transform.localEulerAngles = rotation;
    }
}