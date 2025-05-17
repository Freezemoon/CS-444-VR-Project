using UnityEngine;

public class LookAt : MonoBehaviour
{
    public Transform target;
    public float yOffsetDegrees = 0f;

    void Update()
    {
        if (target)
        {
            // Flat direction toward the target
            Vector3 lookPosition = new Vector3(target.position.x, transform.position.y, target.position.z);
            
            // Compute rotation that looks toward target
            Quaternion lookRotation = Quaternion.LookRotation(lookPosition - transform.position);

            // Apply Y-axis offset (around the up axis)
            Quaternion offset = Quaternion.Euler(0, yOffsetDegrees, 0);
            transform.rotation = lookRotation * offset;
        }
    }
}
