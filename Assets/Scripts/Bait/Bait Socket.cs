using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;

public class BaitSocket : MonoBehaviour
{
    private XRSocketInteractor socket;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnSocketConnected);
    }

    private void OnSocketConnected(SelectEnterEventArgs args)
    {
        GameObject insertedObject = args.interactableObject.transform.gameObject;
        GameObject socketObject = gameObject;

        // Retrieve level from each object
        int levelA = insertedObject.GetComponent<BaitValue>()?.level ?? 0;
        int levelB = socketObject.GetComponent<BaitValue>()?.level ?? 0;

        Debug.Log($"object level: {levelA} has been merged with object level: {levelB}");

        StoreBait(levelA, levelB);

        // destroy both objects after a delay
        StartCoroutine(DestroyAfterDelay(insertedObject, socketObject, 0.1f));
    }

    private IEnumerator DestroyAfterDelay(GameObject objA, GameObject objB, float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(objA);
        Destroy(objB);
    }

    private void StoreBait(int a, int b)
    {
        // TODO : add bait to inventory
    }
}
