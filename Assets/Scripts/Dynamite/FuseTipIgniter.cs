using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FuseTipIgniter : MonoBehaviour
{
    [Tooltip("Reference back to the parent Dynamite script")]
    [SerializeField] private Dynamite dynamite;

    void Awake()
    {
        // Ensure this really is a trigger
        var col = GetComponent<Collider>();
        col.isTrigger = true;

        if (dynamite == null)
            Debug.LogWarning("FuseTipIgniter: assign the Dynamite reference in the Inspector.", this);
    }

    void OnTriggerEnter(Collider other)
    {
        if (_dynamiteIsReady && other.CompareTag("Zippo"))
        {
            dynamite.Ignite();
        }
    }

    private bool _dynamiteIsReady => dynamite != null;
}