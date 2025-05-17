using UnityEngine;

public class VerticalBounce : MonoBehaviour
{
    public float amplitude = 0.1f;
    public float frequency = 5f;

    private Vector3 initialPosition;

    void Start()
    {
        initialPosition = transform.localPosition;
    }

    void Update()
    {
        float offsetY = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = initialPosition + new Vector3(0, offsetY, 0);
    }
}
