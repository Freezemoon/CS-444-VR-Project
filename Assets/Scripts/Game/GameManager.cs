using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace Game
{
    /// <summary>
    /// Singleton  qui permet de gérer le GameState et les systèmes du jeu.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance { get; private set; }
        public GameObject spawnFishPrefab;
        public Transform initSpawnFishPos;

        public GameObject typewriterEffectCanvas;
        public TypewriterEffect typewriterEffect;
        public AudioSource larryAudioSource;
        public List<AudioClip> larrySounds;
        public AudioClip larrySoundMumble;
        public GameObject larryTextButton;

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
        
        public GameState State = new();

        public enum DialogueState
        {
            IntroStart,
            IntroFishingRodGrabbed,
            IntroReel,
            IntroAimBubble,
            IntroWaitingFish,
            IntroPullFight,
            IntroReelFight,
            IntroAlternatePullReel,
            IntroGrabFish,
            IntroDropFishInBucket,
            IntroHopOnBoat,
            IntroPressStartBoat,
        }

        private float _currentTextTime;
        private float _neededTextTime;

        private bool _isCurrentTextDisplay;

        private int _currentTextIndex;
        
        private bool isMumbleEnabled;
        
        private class TextEntry
        {
            public string text;
            public bool isDisplayable;
            public bool activateNextText;
        }

        private List<TextEntry> larryTexts = new()
        {
            new TextEntry {
                text = "Hey! Psst… you! Yeah, you. Over here. Look around the fishing dock… in the water!",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "There you are! Finally, someone with a face that screams “I know how to hold a fishing rod!” " +
                       "…Or at least, someone who’s willing to fake it.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "I’m Larry. I live here. And lately… it’s been a bit crowded in my lake. Too many fish. " +
                       "Not enough personal space. You get it, right?",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "So! I need your help. You're gonna do a little fishing. Help clear some space. " +
                       "Don't worry — I’ll walk you through everything.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "First things first. Turn around and look near the edge of the dock. " +
                       "See that fishing rod? That beauty right there? " +
                       "That’s yours now. Pick it up with your right hand.",
                isDisplayable = false,
                activateNextText = false
            },
            // 5
            // IntroFishingRodGrabbed
            new TextEntry
            {
                text = "Okay, time for the fun part. Hold the select button. " +
                       "Throw like you’re tossing a paper airplane. " +
                       "Then let go to launch the bait. " +
                       "Aim anywhere — just try it out!",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroReel
            new TextEntry
            {
                text = "Time to reel it back. " +
                       "Grab the handle with your left hand. " +
                       "Make sure you're turning the right way — it matters!" +
                       "Keep reeling until the bait's back to you.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroAimBubble
            new TextEntry
            {
                text = "Nice! Now cast again, but look for bubbles on the water. " +
                       "See them? That means a fish is lurking below. " +
                       "Maybe even one of my neighbors. " +
                       "Toss your bait right into that bubbly spot.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroWaitingFish
            new TextEntry
            {
                text = "Now… wait. Patience is key. Fish are shy. Like me on Mondays.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroPullFight
            new TextEntry
            {
                text = "Whoa! You got a bite! Quick — pull back! Show that fish who's boss! " +
                       "Keep pulling... don’t let go yet!",
                isDisplayable = false,
                activateNextText = false
            },
            // 10
            // IntroReelFight
            new TextEntry
            {
                text = "Alright — now reel it in!",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroAlternatePullReel
            new TextEntry
            {
                text = "Now keep going — pull, then reel, then pull again. " +
                       "Just keep alternating until the fish gives up. " +
                       "You'll feel it when it's done fighting!",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroGrabFish
            new TextEntry
            {
                text = "Nice catch! You did it! " +
                       "Now, while holding the rod, grab the fish with your left hand. " +
                       "Don’t be shy — it won’t bite.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroDropFishInBucket
            new TextEntry
            {
                text = "Then drop it in a bucket like the one near the boat. " +
                       "That’s your storage. Like a fridge… but splashier.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroHopOnBoat
            new TextEntry
            {
                text = "Wanna explore? Hop on the boat — I’ll meet you there!",
                isDisplayable = false,
                activateNextText = false
            },
            // 15
            // IntroPressStartBoat
            new TextEntry
            {
                text = "Surprised? I swim fast... Now press the on/off button to start the boat engine.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "OK, Alex has to do the rest of the dialogue, see you soon my fisherman friend!",
                isDisplayable = false,
                activateNextText = false
            }
        };

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

        private void Start()
        {
            larryTexts[0].isDisplayable = true;
            _isCurrentTextDisplay = true;
            typewriterEffectCanvas.SetActive(false);
            _neededTextTime = 6;
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

        private void Update()
        {
            _currentTextTime += Time.deltaTime;
            
            if (_currentTextIndex < 0 || _currentTextIndex >= larryTexts.Count) return;

            if (_currentTextTime <= _neededTextTime) return;

            if (!larryTexts[_currentTextIndex].isDisplayable) return;
            
            typewriterEffectCanvas.SetActive(true);
            larryTextButton.SetActive(larryTexts[_currentTextIndex].activateNextText);
            typewriterEffect.fullText = larryTexts[_currentTextIndex].text;
            typewriterEffect.StartTyping();

            if (_currentTextIndex < larrySounds.Count && larrySounds[_currentTextIndex])
            {
                larryAudioSource.clip = larrySounds[_currentTextIndex];
                isMumbleEnabled = false;
            }
            else
            {
                larryAudioSource.clip = larrySoundMumble;
                isMumbleEnabled = true;
            }
            larryAudioSource.Play();
            
            _currentTextIndex++;
        }

        public void ConfirmDialogue()
        {
            if (_currentTextIndex >= larryTexts.Count) return;
            larryTexts[_currentTextIndex].isDisplayable = larryTexts[_currentTextIndex-1].activateNextText;
            
            _currentTextTime = 0;
            _neededTextTime = 0;
        }

        public void StopMumble()
        {
            if (!isMumbleEnabled) return;
            larryAudioSource.Pause();
        }

        public void SetDialogueState(DialogueState state, bool canSetToPrevDialogue = false)
        {
            Debug.Log(state);
            int index = 0;
            switch (state)
            {
                case DialogueState.IntroStart:
                    index = 0;
                    break;
                case DialogueState.IntroFishingRodGrabbed:
                    index = 5;
                    break;
                case DialogueState.IntroReel:
                    index = 6;
                    break;
                case DialogueState.IntroAimBubble:
                    index = 7;
                    Instantiate(spawnFishPrefab, initSpawnFishPos.position, Quaternion.identity);
                    break;
                case DialogueState.IntroWaitingFish:
                    index = 8;
                    break;
                case DialogueState.IntroPullFight:
                    index = 9;
                    break;
                case DialogueState.IntroReelFight:
                    index = 10;
                    break;
                case DialogueState.IntroAlternatePullReel:
                    index = 11;
                    break;
                case DialogueState.IntroGrabFish:
                    index = 12;
                    break;
                case DialogueState.IntroDropFishInBucket:
                    index = 13;
                    break;
                case DialogueState.IntroHopOnBoat:
                    index = 14;
                    break;
                case DialogueState.IntroPressStartBoat:
                    index = 15;
                    break;
            }

            if (index >= _currentTextIndex || canSetToPrevDialogue)
            {
                _currentTextIndex = index;
                larryTexts[_currentTextIndex].isDisplayable = true;
                _currentTextTime = 0;
                _neededTextTime = 0;
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