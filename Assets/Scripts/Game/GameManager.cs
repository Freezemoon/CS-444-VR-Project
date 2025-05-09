using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game
{
    /// <summary>
    /// Singleton  qui permet de gérer le GameState et les systèmes du jeu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance { get; private set; }
        public GameState State = new();

        [Header("Spawning Settings")]
        [Tooltip("The prefab to spawn on the water surface")]
        [SerializeField] private GameObject spawnPrefab;

        [Tooltip("Number of instances to spawn")]
        [SerializeField] private int spawnCount = 3;

        [Tooltip("How high above the water to place the spawn")]
        [SerializeField] private float heightAbove;

        [Header("Water Surface")]
        [Tooltip("Collider of the water surface to spawn on")]
        [SerializeField] private Collider waterCollider;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        /// <summary>
        /// Permet de spawn les poissons sur une surface taggée avec "Water".
        /// Système random très simple pour le moment.
        /// TODO pour affiner le système later :
        /// - Prendre en compte la position du joueur pour pas spawn les préfabs trop loin
        /// - Faire en sorte que quand un poisson est pêché, un respawn un nouveau random. Le but du spawn
        /// count devient donc de maintenir un nombre fix d'instance de poisson à pêcher en tout temps
        /// - Prendre en compte les zones innaccessible du lac pour pas spawn là bas (plusieurs tags?)
        /// </summary>
        /// <summary>
        /// Spawn spawnCount copies of spawnPrefab at random points on waterCollider.
        /// </summary>
        public void SpawnOnWater()
        {
            if (waterCollider == null || spawnPrefab == null)
                return;

            var bounds = waterCollider.bounds;
            for (int i = 0; i < spawnCount; i++)
            {
                // pick a random XZ inside the water’s world‐space bounds
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float z = Random.Range(bounds.min.z, bounds.max.z);
                Vector3 origin = new Vector3(x, bounds.max.y + 15f, z);

                // raycast down against only the water collider
                var ray = new Ray(origin, Vector3.down);
                if (waterCollider.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    Vector3 spawnPos = hit.point + Vector3.up * heightAbove;
                    Instantiate(spawnPrefab, spawnPos, Quaternion.identity);
                }
                else
                {
                    i--; // retry if the random point missed the mesh
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