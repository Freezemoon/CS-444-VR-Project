using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UIElements;
using UnityEngine.XR;
using CommonUsages = UnityEngine.XR.CommonUsages;
using InputDevice = UnityEngine.XR.InputDevice;
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

        public GameState State = new();

        [Header("DialogueTiggers")]
        public DialogueTrrigger dialogueTriggerDynamiteBought;

        [Header("Spawning Settings")]
        [Tooltip("The prefab to spawn on the water surface")]
        [SerializeField] private GameObject spawnPrefab;

        [Tooltip("Number of instances to spawn")]
        [SerializeField] private int spawnCount = 5;

        [Tooltip("How high above the water to place the spawn")]
        [SerializeField] private float heightAbove;

        [Header("Water Surface")]
        [Tooltip("Collider of the water surface to spawn on")]
        [SerializeField] private Collider waterCollider;
        
        [Header("Medium/Hard Spawning Settings")]
        [Tooltip("Collider of the hard‐zone surface to spawn on.")]
        [SerializeField] private Collider hardZoneCollider;
        [Tooltip("How many medium/hard fish to keep alive at once.")]
        [SerializeField] private int hardSpawnCount = 5;

        public int currentTextIndex { get; private set; }

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
            DynamiteBought,
            RockDynamite,
            DynamiteSpawned,
            ThrowDynamite,
            RockExploded,
            WelcomeHome,
            SecondIslandMeetLarry,
            CraftBaits,
            EquipBait,
            ReadyToFishMore,
            Victory,
        }

        private float _currentTextTime;
        private float _neededTextTime;

        private bool _isCurrentTextDisplay;
        
        private bool isMumbleEnabled;
        
        private class TextEntry
        {
            public string text;
            public bool isDisplayable;
            public bool activateNextText;
        }

        private List<TextEntry> larryTexts = new()
        {
            // Start
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
            // FishingRodGrabbed
            new TextEntry
            {
                text = "Okay, time for the fun part. Hold the select button.\n" +
                       "Throw like you’re tossing a paper airplane.\n" +
                       "Then let go to launch the bait.\n" +
                       "Aim anywhere — just try it out!",
                isDisplayable = false,
                activateNextText = false
            },
            // Reel
            new TextEntry
            {
                text = "Time to reel it back.\n" +
                       "Grab the handle with your left hand.\n" +
                       "Make sure you're turning the right way — it matters!\n" +
                       "Keep reeling until the bait's back to you.",
                isDisplayable = false,
                activateNextText = false
            },
            // AimBubble
            new TextEntry
            {
                text = "Nice! Now cast again, but look for bubbles on the water.\n" +
                       "See them? That means a fish is lurking below.\n" +
                       "Maybe even one of my neighbors.\n" +
                       "Toss your bait right into that bubbly spot.",
                isDisplayable = false,
                activateNextText = false
            },
            // WaitingFish
            new TextEntry
            {
                text = "Now… wait. Patience is key.\n" +
                       "Fish are shy.\n" +
                       "Like me on Mondays.",
                isDisplayable = false,
                activateNextText = false
            },
            // PullFight
            new TextEntry
            {
                text = "Whoa! You got a bite! Quick — pull back!\n" +
                       "Show that fish who's boss!\n" +
                       "Keep pulling... don’t let go yet!",
                isDisplayable = false,
                activateNextText = false
            },
            // 10
            // ReelFight
            new TextEntry
            {
                text = "Alright — now reel it in!",
                isDisplayable = false,
                activateNextText = false
            },
            // AlternatePullReel
            new TextEntry
            {
                text = "Now keep going — pull, then reel, then pull again.\n" +
                       "Just keep alternating until the fish gives up.\n" +
                       "You'll feel it when it's done fighting!",
                isDisplayable = false,
                activateNextText = false
            },
            // GrabFish
            new TextEntry
            {
                text = "Nice catch! You did it!\n" +
                       "Now, grab the fish.\n" +
                       "Don’t be shy — it won’t bite.",
                isDisplayable = false,
                activateNextText = false
            },
            // DropFishInBucket
            new TextEntry
            {
                text = "Then drop it in a bucket like the one here on the piece of wood.\n" +
                       "That’s your storage.\n" +
                       "Like a fridge… but splashier.",
                isDisplayable = false,
                activateNextText = false
            },
            // HopOnBoat
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
                       "If you ever lose your fishing rod… just press the A button on your right controller.\n" +
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
                activateNextText = false
            },
            // DynamiteBought
            // TODO: When the player buys a dynamite
            new TextEntry
            {
                text = "Nice! You found the dynamite!\n" +
                       "Now head back to that big rock — I’ll meet you there and explain what’s next.",
                isDisplayable = false,
                activateNextText = false
            },
            // RockDynamite
            // TODO: When the player goes to the rock with at least one dynamite in his inventory
            new TextEntry
            {
                text = "Hey again!\n" +
                       "Alright, now open your inventory — that’s the X button on your left controller.\n" +
                       "Select the dynamite.",
                isDisplayable = false,
                activateNextText = false
            },
            // DynamiteSpawned
            // TODO: When the player spawns a dynamite
            new TextEntry
            {
                text = "Okay, listen carefully now!\n" +
                       "First, grab the dynamite with one hand.\n" +
                       "Then, use the lighter in your other hand to light the fuse.\n" +
                       "Once it starts burning… aim for the rock — and toss it!",
                isDisplayable = false,
                activateNextText = false
            },
            // 30
            // ThrowDynamite
            // TODO: When the player lighted a dynamite
            new TextEntry
            {
                text = "Do it! Throw the dynamite at the rock! QUICK!",
                isDisplayable = false,
                activateNextText = false
            },
            // RockExploded
            // TODO: When the player exploded the rock with a dynamite
            new TextEntry
            {
                text = "Boom! Good job!\n" +
                       "You cleared the way — you can now reach the other side of the lake.\n" +
                       "Let’s meet over there!",
                isDisplayable = false,
                activateNextText = false
            },
            // WelcomeHome
            // TODO: When the player reaches the other side of the lake
            new TextEntry
            {
                text = "Welcome home! Haha.\n" +
                       "This is my side of the lake — but beware: the fish here are a bit tougher.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "Catch a few and meet me again near the small island —\n" +
                       "just by the bridge and the little house in the middle of this lake.",
                isDisplayable = false,
                activateNextText = false
            },
            // SecondIslandMeetLarry
            // TODO: When the player meet Larry near the second island
            new TextEntry
            {
                text = "So, they’re harder to fish, huh?",
                isDisplayable = false,
                activateNextText = true
            },
            // 35
            new TextEntry
            {
                text = "Well, now’s the perfect time to tell you: there are better baits at the shop.\n" +
                       "Yep — the same shop near the dock where you bought the dynamites.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "Those baits will make fishing a whole lot easier.\n" +
                       "Sell the fish you’ve caught so far, and buy yourself some upgrades.\n" +
                       "See you back there soon!",
                isDisplayable = false,
                activateNextText = false
            },
            // CraftBaits
            // TODO: When the player buys a bait
            new TextEntry
            {
                text = "Oh! You found the baits!\n" +
                       "There are tons of combos — and yep, you gotta craft them.\n" +
                       "Just grab a piece in each hand and snap ’em together. Easy!",
                isDisplayable = false,
                activateNextText = false
            },
            // EquipBait
            // TODO: When the player needs to equip a bait
            new TextEntry
            {
                text = "Great job! Your first custom bait!\n" +
                       "Now open your inventory again — X button, left controller —\n" +
                       "and select the bait you just crafted.",
                isDisplayable = false,
                activateNextText = false
            },
            // ReadyToFishMore
            // TODO: When the player equiped his bait
            new TextEntry
            {
                text = "Perfect! You're all set now.\n" +
                       "Go out and fish some more!",
                isDisplayable = false,
                activateNextText = true
            },
            // 40
            new TextEntry
            {
                text = "Catch 5 of each kind of fish — that should clear a good patch of space for me.\n" +
                       "Go on, partner — you’ve got this!",
                isDisplayable = false,
                activateNextText = false
            },
            // Victory
            // TODO: When the player caught 5 of each kind of fish
            new TextEntry
            {
                text = "Hey! You did it!\n" +
                       "You caught at least five of each kind — I can finally stretch again!\n" +
                       "Ahhh… feels so much better already.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "You’ve helped me out more than you know.\n" +
                       "That’s everything I needed — your mission’s complete!",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "If you want, stick around and fish some more — totally up to you.\n" +
                       "But from here on out, it’s all extra. I’m heading home!",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "You’re free to explore, fish, or craft just for fun now.\n" +
                       "Or take a break — your work here is done, hero of the lake!",
                isDisplayable = false,
                activateNextText = true
            },
            // 45
            new TextEntry
            {
                text = "Alright... bye now! Swim safe!",
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
            // Initial spawn
            SpawnOnWater();
            SpawnOnHardZone();
            larryTexts[0].isDisplayable = true;
            _isCurrentTextDisplay = true;
            typewriterEffectCanvas.SetActive(false);
            _neededTextTime = 0;
            currentTextIndex = -1;
        }
        
        /// <summary>
        /// Called when a fish with FishDeathNotifier is destroyed; spawns a replacement.
        /// </summary>
        private void OnFishDestroyed(FishingArea deadFish)
        {
            // Unsubscribe
            deadFish.onDeath -= OnFishDestroyed;
            // Spawn a single replacement
            SpawnOne();
        }

        /// <summary>
        /// Permet de générer le spawn initial des zones de pêche.
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
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float z = Random.Range(bounds.min.z, bounds.max.z);
                Vector3 origin = new Vector3(x, bounds.max.y + 500f, z);

                Ray ray = new Ray(origin, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    // skip hits that aren’t the water surface
                    if (hit.collider != waterCollider)
                    {
                        i--;
                        continue;
                    }

                    Vector3 spawnPos = hit.point + Vector3.up * heightAbove;
                    var fishGo = Instantiate(spawnPrefab, spawnPos, Quaternion.identity);
                    var area   = fishGo.GetComponent<FishingArea>();
                    if (area != null)
                    {
                        area.areaDifficulty = FishingGame.Difficulty.Easy;
                        area.onDeath += OnFishDestroyed;
                    }
                }
                else
                {
                    // missed everything, retry
                    i--;
                }
            }
        }

        private void Update()
        {
            _currentTextTime += Time.deltaTime;
            
            if (currentTextIndex < 0 || currentTextIndex >= larryTexts.Count) return;

            if (_currentTextTime <= _neededTextTime) return;

            if (!larryTexts[currentTextIndex].isDisplayable) return;
            
            typewriterEffectCanvas.SetActive(true);
            larryTextButton.SetActive(false);
            typewriterEffect.fullText = larryTexts[currentTextIndex].text;
            typewriterEffect.StartTyping();

            if (currentTextIndex < larrySounds.Count && larrySounds[currentTextIndex])
            {
                larryAudioSource.clip = larrySounds[currentTextIndex];
                isMumbleEnabled = false;
            }
            else
            {
                larryAudioSource.clip = larrySoundMumble;
                isMumbleEnabled = true;
            }
            larryAudioSource.Play();
            
            currentTextIndex++;
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private void OnYButtonPressed()
        {
            Debug.Log("X was pressed!");
            // … your logic here …
        }

        public void ConfirmDialogue()
        {
            if (currentTextIndex >= larryTexts.Count) return;
            larryTexts[currentTextIndex].isDisplayable = larryTexts[currentTextIndex-1].activateNextText;
            
            _currentTextTime = 0;
            _neededTextTime = 0;
        }

        public void StopMumble()
        {
            if (!isMumbleEnabled) return;
            larryAudioSource.Pause();
            larryTextButton.SetActive(larryTexts[currentTextIndex-1].activateNextText);
        }

        public void restartFishingTutoIfLostBeforeGrabFish()
        {
            if (currentTextIndex <= 12)
            {
                SetDialogueState(DialogueState.AimBubble, true);
            }
        }

        public void SetDialogueState(DialogueState state, bool canSetToPrevDialogue = false)
        {
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
                case DialogueState.DynamiteBought:
                    index = 27;
                    break;
                case DialogueState.RockDynamite:
                    index = 28;
                    break;
                case DialogueState.DynamiteSpawned:
                    index = 29;
                    break;
                case DialogueState.ThrowDynamite:
                    index = 30;
                    break;
                case DialogueState.RockExploded:
                    index = 31;
                    break;
                case DialogueState.WelcomeHome:
                    index = 32;
                    break;
                case DialogueState.SecondIslandMeetLarry:
                    index = 34;
                    break;
                case DialogueState.CraftBaits:
                    index = 37;
                    break;
                case DialogueState.EquipBait:
                    index = 38;
                    break;
                case DialogueState.ReadyToFishMore:
                    index = 39;
                    break;
                case DialogueState.Victory:
                    index = 41;
                    break;
            }

            if (index >= currentTextIndex || canSetToPrevDialogue)
            {
                currentTextIndex = index;
                
                if (canSetToPrevDialogue)
                {
                    for (int i = currentTextIndex + 1; i < larryTexts.Count; i++)
                    {
                        larryTexts[i].isDisplayable = false;
                    }
                }
                
                larryTexts[currentTextIndex].isDisplayable = true;
                _currentTextTime = 0;
                _neededTextTime = 0;
                
                switch (state)
                {
                    case DialogueState.AimBubble:
                        Instantiate(spawnFishPrefab, initSpawnFishPos.position, Quaternion.identity);
                        break;
                    case DialogueState.DynamiteBought:
                        dialogueTriggerDynamiteBought.ValidateDialogue();
                        break;
                }
            }
        }

        
        /// <summary>
        /// Spawns a single fish at a random point on the waterCollider.
        /// </summary>
        private void SpawnOne()
        {
            if (!waterCollider || !spawnPrefab)
                return;

            var bounds = waterCollider.bounds;
            while (true)
            {
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float z = Random.Range(bounds.min.z, bounds.max.z);
                Vector3 origin = new Vector3(x, bounds.max.y + 500f, z);

                Ray ray = new Ray(origin, Vector3.down);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    if (hit.collider != waterCollider)
                        continue;

                    Vector3 spawnPos = hit.point + Vector3.up * heightAbove;
                    var fishGo = Instantiate(spawnPrefab, spawnPos, Quaternion.identity);
                    var area = fishGo.GetComponent<FishingArea>();
                    if (area)
                        area.onDeath += OnFishDestroyed;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Spawns medium or hard prefabs at random points on hardZoneCollider.
        /// </summary>
        public void SpawnOnHardZone()
        {
            if (hardZoneCollider == null || spawnPrefab == null)
                return;

            var bounds = hardZoneCollider.bounds;
            for (int i = 0; i < hardSpawnCount; i++)
                SpawnOneHard(bounds);
        }

        private void SpawnOneHard(Bounds bounds)
        {
            while (true)
            {
                float x = Random.Range(bounds.min.x, bounds.max.x);
                float z = Random.Range(bounds.min.z, bounds.max.z);
                Vector3 origin = new Vector3(x, bounds.max.y + 500f, z);
                Ray ray = new Ray(origin, Vector3.down);

                if (Physics.Raycast(ray, out var hit, Mathf.Infinity) && hit.collider == hardZoneCollider)
                {
                    // decide difficulty: 65% medium, 35% hard
                    var difficulty = Random.value < 0.65f
                        ? FishingGame.Difficulty.Medium
                        : FishingGame.Difficulty.Hard;

                    Vector3 spawnPos = hit.point + Vector3.up * heightAbove;
                    var go = Instantiate(spawnPrefab, spawnPos, Quaternion.identity);

                    var area = go.GetComponent<FishingArea>();
                    if (area != null)
                    {
                        area.areaDifficulty = difficulty;
                        area.onDeath += OnHardFishDestroyed;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// When one medium/hard fish dies, spawn a replacement in the hard zone.
        /// </summary>
        private void OnHardFishDestroyed(FishingArea deadFish)
        {
            deadFish.onDeath -= OnHardFishDestroyed;
            if (hardZoneCollider)
                SpawnOneHard(hardZoneCollider.bounds);
        }
        
        public int GetMoney() => State.Money;
        public void AddMoney(int amount) => State.Money += amount;

        public int GetBucketValue() => State.BucketValue;
        public void AddToBucket(int value) => State.BucketValue += value;

        public void SellBucket()
        {
            AddMoney(State.BucketValue);
            State.BucketValue = 0;
        }

        public void BuyCart(int amount) => State.Money -= amount;
        public void ChangeFishingRod(FishingRodStats fishingRod) => State.CurrentRod = fishingRod;
        public FishingRodStats GetPlayerRod() => State.CurrentRod;

        public int GetComponent1Amount() => State.Component1Amount;
        public void AddComponent1Amount(int amount) => State.Component1Amount += amount;

        public int GetComponent2Amount() => State.Component2Amount;
        public void AddComponent2Amount(int amount) => State.Component2Amount += amount;

        public int GetComponent3Amount() => State.Component3Amount;
        public void AddComponent3Amount(int amount) => State.Component3Amount += amount;

        public int GetComponent4Amount() => State.Component4Amount;
        public void AddComponent4Amount(int amount) => State.Component4Amount += amount;

        public int GetComponent5Amount() => State.Component5Amount;
        public void AddComponent5Amount(int amount) => State.Component5Amount += amount;

        public int GetComponent6Amount() => State.Component6Amount;
        public void AddComponent6Amount(int amount) => State.Component6Amount += amount;

        public int GetDynamiteAmount() => State.DynamiteAmount;
        public void AddDynamiteAmount(int amount) => State.DynamiteAmount += amount;

        
    }
}