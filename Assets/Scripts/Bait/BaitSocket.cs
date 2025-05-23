using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections;
using Game;

public class BaitSocket : MonoBehaviour
{
    private XRSocketInteractor socket;
    public AudioClip audioClip;

    void Awake()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnSocketConnected);
    }

    private void OnSocketConnected(SelectEnterEventArgs args)
    {
        GameManager.instance.SetDialogueState(GameManager.DialogueState.EquipBait);
        
        GameObject insertedObject = args.interactableObject.transform.gameObject;
        GameObject socketObject = gameObject;

        // Retrieve level from each object
        int levelA = insertedObject.GetComponent<BaitValue>()?.level ?? 0;
        int levelB = socketObject.GetComponent<BaitValue>()?.level ?? 0;

        if (audioClip != null) {
            AudioSource.PlayClipAtPoint(audioClip, transform.position);
        }
        
        Instantiate(FishingGame.instance.conffetiParticleSystem, transform.position, Quaternion.identity);
        
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
        // determine which param is hook (1–3) and which is cork (10–30)
        int hook  = (a >= 1 && a <= 3)  ? a : (b >= 1 && b <= 3)  ? b : -1;
        int cork  = (a >= 10 && a <= 30 && a % 10 == 0) ? a
            : (b >= 10 && b <= 30 && b % 10 == 0) ? b
            : -1;

        if (hook == -1 || cork == -1)
        {
            Debug.LogWarning($"Invalid inputs: a={a}, b={b}");
            return;
        }

        switch ((hook, cork))
        {
            case (1, 10): 
                //full blue; 
                GameManager.instance.State.BBBaitCount++;
                break;
            case (1, 20): 
                // blue hook green cork
                GameManager.instance.State.BGBaitCount++;
                break;
            case (1, 30):
                // blue hook red cork
                GameManager.instance.State.BRBaitCount++;
                break;
            case (2, 10): 
                // green hook blue cork
                GameManager.instance.State.GBBaitCount++;
                break;
            case (2, 20): 
                // full green
                GameManager.instance.State.GGBaitCount++;
                break;
            case (2, 30): 
                // green hook red cork
                GameManager.instance.State.GRBaitCount++;
                break;
            case (3, 10): 
                // red hook blue cork
                GameManager.instance.State.RBBaitCount++;
                break;
            case (3, 20): 
                //red hook green cork 
                GameManager.instance.State.RGBaitCount++;
                break;
            case (3, 30): 
                // full red
                GameManager.instance.State.RRBaitCount++;
                break;
        }
    }
}
