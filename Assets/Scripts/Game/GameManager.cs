using UnityEngine;
using UnityEngine.SceneManagement;

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

        private GameObject _spawnPrefab; // Le prefab qui se fait spawn par le spawner
        private Collider _waterCollider; // Le collider de l'eau pour les poissons
        private int _spawnCount = 3; // Combien de poissons sont présent en simultané.

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
            
            // Permet de charger le prefab qu'on veut faire spawn
            // TODO : Changer pour le préfab voulu
            _spawnPrefab = Resources.Load<GameObject>("Prefabs/Debug/Sphere");
            if (_spawnPrefab == null)
                Debug.LogError("Could not find MySpawnPrefab in Resources/Prefabs/");
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDestroy()
        {
            // clean up
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        // called whenever a new scene is loaded
        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // try to grab the Water object if it exists in this scene
            var waterGo = GameObject.FindWithTag("Water");
            if (waterGo != null)
            {
                _waterCollider = waterGo.GetComponent<Collider>();
                if (_waterCollider == null)
                    Debug.LogError("Found Water tag, but no Collider!", waterGo);

            }
            else
            {
                // no water in this scene—clear your reference if you like
                _waterCollider = null;
            }
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
        
        /// <summary>
        /// Permet de spawn les poissons sur une surface taggée avec "Water".
        /// Système random très simple pour le moment.
        /// </summary>
        public void SpawnOnWater()
        {
            if (_waterCollider == null)
                return;

            var bounds = _waterCollider.bounds;
            for (int i = 0; i < _spawnCount; i++)
            {
                // Pick a random XZ point inside the water’s world‐space bounds
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float z = Random.Range(bounds.min.z, bounds.max.z);
                // Start the ray well above the top of the bounds
                Vector3 origin = new Vector3(x, bounds.max.y + 15f, z);

                // Build a downward ray
                Ray ray = new Ray(origin, Vector3.down);
                // Perform the raycast against the water collider only, so we don't register hits for other objects
                if (_waterCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    // hit.point is guaranteed to lie on the water mesh
                    Vector3 spawnPos = hit.point;
                    Instantiate(_spawnPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    // Missed the water collider (e.g. point was outside the mesh), retry
                    i--;
                }
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