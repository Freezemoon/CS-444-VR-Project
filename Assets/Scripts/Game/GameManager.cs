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
            Start,
            FishingRodGrabbed,
            Reel,
            AimBubble,
            WaitingFish,
            PullFight,
            ReelFight,
            AlternatePullReel,
            GrabFish,
            DropFishInBucket,
            HopOnBoat,
            PressStartBoat,
            AccelerateBoat,
            ReverseBoat,
            MeetBehindSmallIsland,
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
                text = "Hey! Psst… you!\n" +
                       "Yeah, you.\n" +
                       "Over here.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "There you are!\n" +
                       "Finally, someone with a face that screams “I know how to hold a fishing rod!”\n" +
                       "…Or at least, someone who’s willing to fake it.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "I’m Larry. I live here.\n" +
                       "And lately… it’s been a bit crowded in my lake. Too many fish. " +
                       "Not enough personal space.\n" +
                       "You get it, right?",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "So! I need your help.\n" +
                       "You're gonna do a little fishing. Help clear some space.\n" +
                       "Don't worry — I’ll walk you through everything.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "First things first. Turn around and look near the rock on the right.\n" +
                       "See that fishing rod? That beauty right there?\n" +
                       "That’s yours now. Pick it up with your right hand.",
                isDisplayable = false,
                activateNextText = false
            },
            // 5
            // IntroFishingRodGrabbed
            new TextEntry
            {
                text = "Okay, time for the fun part. Hold the select button.\n" +
                       "Throw like you’re tossing a paper airplane.\n" +
                       "Then let go to launch the bait.\n" +
                       "Aim anywhere — just try it out!",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroReel
            new TextEntry
            {
                text = "Time to reel it back.\n" +
                       "Grab the handle with your left hand.\n" +
                       "Make sure you're turning the right way — it matters!\n" +
                       "Keep reeling until the bait's back to you.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroAimBubble
            new TextEntry
            {
                text = "Nice! Now cast again, but look for bubbles on the water.\n" +
                       "See them? That means a fish is lurking below.\n" +
                       "Maybe even one of my neighbors.\n" +
                       "Toss your bait right into that bubbly spot.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroWaitingFish
            new TextEntry
            {
                text = "Now… wait. Patience is key.\n" +
                       "Fish are shy.\n" +
                       "Like me on Mondays.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroPullFight
            new TextEntry
            {
                text = "Whoa! You got a bite! Quick — pull back!\n" +
                       "Show that fish who's boss!\n" +
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
                text = "Now keep going — pull, then reel, then pull again.\n" +
                       "Just keep alternating until the fish gives up.\n" +
                       "You'll feel it when it's done fighting!",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroGrabFish
            new TextEntry
            {
                text = "Nice catch! You did it!\n" +
                       "Now, grab the fish.\n" +
                       "Don’t be shy — it won’t bite.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroDropFishInBucket
            new TextEntry
            {
                text = "Then drop it in a bucket like the one here on the piece of wood.\n" +
                       "That’s your storage.\n" +
                       "Like a fridge… but splashier.",
                isDisplayable = false,
                activateNextText = false
            },
            // IntroHopOnBoat
            new TextEntry
            {
                text = "Wanna explore?\n" +
                       "See the dock a bit further away, on the right, with the boat?\n" +
                       "Hop on the boat — I’ll meet you there!",
                isDisplayable = false,
                activateNextText = false
            },
            // 15
            // PressStartBoat
            new TextEntry
            {
                text = "Surprised? I swim fast...\n" +
                       "Now press the on/off button to start the boat engine.",
                isDisplayable = false,
                activateNextText = false
            },
            // AccelerateBoat
            new TextEntry
            {
                text = "Grab the boat handle. Hold the select button to accelerate.\n" +
                       "Want to steer?\n" +
                       "Move the handle left to go right. Move it right to go left.\n" +
                       "Yeah, it’s weird. You’ll get used to it.",
                isDisplayable = false,
                activateNextText = false
            },
            // ReverseBoat
            new TextEntry
            {
                text = "Need to go backward? There’s a reverse button for that.\n" +
                       "Just in case you bump into a rock. Or a duck.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "Now go explore the lake a bit.\n" +
                       "Look for fishing spots — you’ll know them by the bubbles.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "Oh, by the way —\n" +
                       "If you ever lose your fishing rod… just press A.\n" +
                       "Could save you a swim.",
                isDisplayable = false,
                activateNextText = true
            },
            // 20
            new TextEntry
            {
                text = "When you’re ready…\n" +
                       "Meet me behind the small island in the middle of the lake.\n" +
                       "I’ll be waiting!",
                isDisplayable = false,
                activateNextText = false
            },
            // MeetBehindSmallIsland
            new TextEntry
            {
                text = "Hey, you made it!\n" +
                       "How’s the fishing? Getting the hang of it?\n" +
                       "Caught anything particularly annoying yet? Heh.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "See that big rock over there, blocking the access to the rest of the lake?\n" +
                       "Yeah… not great. You and your boat can’t get through.\n" +
						"Us fish? Oh, we’ve got our sneaky ways around it.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "It’s blocking the way to the big part of the lake — that’s where I live.\n" +
                       "Way more fish over there… and way less space for me. Not ideal.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "Buuut I have an idea.\n" +
                       "Head back to the fishing dock. There’s a little shop nearby.\n" +
                       "You can sell your fish there — and maybe buy a few… tools.",
                isDisplayable = false,
                activateNextText = true
            },
            // 25
            new TextEntry
            {
                text = "Dynamite, for example.\n" +
                       "Totally legal. Totally safe.\n" +
                       "And hey — they might help with fishing too.\n" +
                       "Haha! Anyway...",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "Let me know when you're all stocked up!",
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
            _neededTextTime = 0;
            _currentTextIndex = -1;
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
            larryTextButton.SetActive(false);
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
            larryTextButton.SetActive(larryTexts[_currentTextIndex-1].activateNextText);
        }

        public void SetDialogueState(DialogueState state, bool canSetToPrevDialogue = false)
        {
            Debug.Log(state);
            int index = 0;
            switch (state)
            {
                case DialogueState.Start:
                    index = 0;
                    break;
                case DialogueState.FishingRodGrabbed:
                    index = 5;
                    break;
                case DialogueState.Reel:
                    index = 6;
                    break;
                case DialogueState.AimBubble:
                    index = 7;
                    Instantiate(spawnFishPrefab, initSpawnFishPos.position, Quaternion.identity);
                    break;
                case DialogueState.WaitingFish:
                    index = 8;
                    break;
                case DialogueState.PullFight:
                    index = 9;
                    break;
                case DialogueState.ReelFight:
                    index = 10;
                    break;
                case DialogueState.AlternatePullReel:
                    index = 11;
                    break;
                case DialogueState.GrabFish:
                    index = 12;
                    break;
                case DialogueState.DropFishInBucket:
                    index = 13;
                    break;
                case DialogueState.HopOnBoat:
                    index = 14;
                    break;
                case DialogueState.PressStartBoat:
                    index = 15;
                    break;
                case DialogueState.AccelerateBoat:
                    index = 16;
                    break;
                case DialogueState.ReverseBoat:
                    index = 17;
                    break;
                case DialogueState.MeetBehindSmallIsland:
                    index = 21;
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