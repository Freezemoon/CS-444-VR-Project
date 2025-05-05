using UnityEngine;

namespace Game
{
    /// <summary>
    /// Singleton (est unique et instantié automatiquement et persiste tout le jeu donc) qui permet
    /// de gérer le GameState et les systèmes du jeu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance { get; private set; }
        public GameState State = new();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager Awake");
        }

        // Permet d'ajouter le singleton dès le lancement du jeu (ça load avant la scène)
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitOnLoad()
        {
            if (instance == null)
            {
                GameObject go = new GameObject("GameManager");
                go.AddComponent<GameManager>();
            }
        }
        
        public int GetMoney() => State.Money;
        public void AddMoney(int amount) => State.Money += amount;

        public int GetBucketValue() => State.BucketValue;
        public void AddToBucket(int value) => State.BucketValue += value;
        
        public void ChangeFishingRod(FishingRodStats fishingRod) => State.CurrentRod = fishingRod;
        public FishingRodStats GetPlayerRod() => State.CurrentRod;
    }
}