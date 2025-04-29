using UnityEngine;

public class FishingLine : MonoBehaviour
{
    public Transform rodTip;
    public Transform bait;
    
    private LineRenderer _lineRenderer;
    
    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = 2;
    }
    
    private void Update()
    {
        _lineRenderer.SetPosition(0, rodTip.position);
        _lineRenderer.SetPosition(1, bait.position);
    }
}
