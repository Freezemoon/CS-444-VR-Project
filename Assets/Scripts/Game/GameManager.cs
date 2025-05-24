using System.Collections.Generic;
using UnityEngine;
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

        public Transform larry;

        public GameState State = new();

        [Header("DialogueTiggers")]
        public DialogueTrrigger dialogueTriggerDynamiteBought;
        public DialogueTrrigger dialogueTriggerCraftBaits;

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
        
        [Header("Fish value")]
        [Tooltip("This sets the value of each fish.")]
        [SerializeField] private int easyFishValue = 50;
        [SerializeField] private int mediumFishValue = 70;
        [SerializeField] private int hardFishValue = 100;

        public int currentTextIndex { get; private set; }
        
        public Vector3 lastFishSpawnerPosition;
        
        [Header("Default fish bait")]
        [SerializeField] private BaitMenu baitMenu;

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
                text = "Alright, time for the fun part! It works like a real rod.\n" +
                       "The bait at the tip moves on its own and sets the direction.\n" +
                       "Hold the select button, give it a swing, and let go to cast.\n" +
                       "Try it out anywhere for now!",
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
                text = "Nice! Try casting a few more times — anywhere you like.\n" +
                       "Then look for bubbles — means a fish is lurking below.\n" +
                       "Maybe even one of my neighbors.\n" +
                       "Toss your bait right into that bubbly spot!",
                isDisplayable = false,
                activateNextText = false
            },
            // WaitingFish
            new TextEntry
            {
                text = "Now… wait. Patience is key.\n" +
                       "Fish are shy.\n" +
                       "Like me at a seaweed speed-dating event.",
                isDisplayable = false,
                activateNextText = false
            },
            // PullFight
            new TextEntry
            {
                text = "Whoa! You got a bite! Quick — pull back!\n" +
                       "Do a fast, sharp motion — bring your arm back over your shoulder!\n" +
                       "Show that fish who's boss!\n" +
                       "Keep pulling… don’t let go yet!",
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
                       "Keep switching between the two — pull, reel, pull, reel.\n" +
                       "You’ll know it’s over when the fish stops fighting!",
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
                       "Especially useful when you accidentally park on a fish’s front porch.",
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
                       "Rod disappeared? Just press the A button on your right controller.\n" +
                       "Boom. Rod's back. No swimming required. We don’t question it.\n" +
                       "I talk, rods teleport — that’s just how things work around here.",
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
                       "How’s the rod treating you?\n" +
                       "Any fish try to pull you into the lake yet?",
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
                       "Head back to the fishing dock and look for a 'Shop' sign — " +
                       "it’s near a climbing wall on the hill.",
                isDisplayable = false,
                activateNextText = true
            },
            // 25
            new TextEntry
            {
                text = "To reach the shop, climb up using the small rocks on the cliff — " +
                       "one hand after the other.\n" +
                       "Don’t worry, the rocks are grab-friendly. Probably.",
                isDisplayable = false,
                activateNextText = true
            },
            new TextEntry
            {
                text = "Once you’re up there, sell your fish — maybe grab a few… tools.\n" +
                       "Dynamite, for example. Totally legal. Totally safe.\n" +
                       "And hey — might help with fishing too. Haha!\n" +
                       "Let me know when you're all stocked up!",
                isDisplayable = false,
                activateNextText = false
            },
            // DynamiteBought
            new TextEntry
            {
                text = "Nice! You found the dynamite!\n" +
                       "Now head back to that big rock — I’ll meet you there and explain what’s next.",
                isDisplayable = false,
                activateNextText = false
            },
            // RockDynamite
            new TextEntry
            {
                text = "Hey again!\n" +
                       "Alright, now open your inventory — that’s the X button on your left controller.\n" +
                       "Select the dynamite.",
                isDisplayable = false,
                activateNextText = false
            },
            // DynamiteSpawned
            new TextEntry
            {
                text = "Okay, listen carefully now!\n" +
                       "Step closer to the rock.\n" +
                       "Grab the dynamite with one hand, light it with the other, " +
                       "then toss it once it’s burning!",
                isDisplayable = false,
                activateNextText = false
            },
            // 30
            // ThrowDynamite
            new TextEntry
            {
                text = "Do it! Throw the dynamite at the rock! QUICK!",
                isDisplayable = false,
                activateNextText = false
            },
            // RockExploded
            new TextEntry
            {
                text = "Boom! Good job!\n" +
                       "You cleared the way — you can now reach the other side of the lake.\n" +
                       "Let’s meet over there!",
                isDisplayable = false,
                activateNextText = false
            },
            // WelcomeHome
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
            new TextEntry
            {
                text = "Oh! You found the baits!\n" +
                       "They’re on the table right next to the shop." +
                       "Tons of combos — just grab a piece in each hand, " +
                       "snap ’em together, then let go." +
                       "It’ll go straight to your inventory!",
                isDisplayable = false,
                activateNextText = false
            },
            // EquipBait
            new TextEntry
            {
                text = "Great job! Your first custom bait!\n" +
                       "Now open your inventory again — X button, left controller —\n" +
                       "and select the bait you just crafted.",
                isDisplayable = false,
                activateNextText = false
            },
            // ReadyToFishMore
            new TextEntry
            {
                text = "Perfect! You're all set.\n" +
                       "Each bait has its own perks — and limited durability.\n" +
                       "You can check the details in the shop by clicking on the baits.",
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

        private void Update()
        {
            // Update victory
            if (State.EasyFishCought >= 5 && State.MediumFishCought >= 5 && State.HardFishCought >= 5)
            {
                larry.position = lastFishSpawnerPosition;
                SetDialogueState(DialogueState.Victory);
            }
            
            // Update typewriter
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

        public void RestartFishingTutoIfLostBeforeGrabFish()
        {
            if (currentTextIndex <= 12)
            {
                SetDialogueState(DialogueState.AimBubble, true);
            }
        }

        public bool SetDialogueState(DialogueState state, bool canSetToPrevDialogue = false)
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
                    case DialogueState.CraftBaits:
                        dialogueTriggerCraftBaits.ValidateDialogue();
                        break;
                }

                return true;
            }

            return false;
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

        public int GetDynamiteAmount() => State.DynamiteAmount;
        public void AddDynamiteAmount(int amount) => State.DynamiteAmount += amount;

        public void HandleCaughtFish(FishingGame.Difficulty diff)
        {
            switch (diff)
            {
                case FishingGame.Difficulty.Easy:
                    AddToBucket(easyFishValue);
                    State.EasyFishCought++;
                    break;
                
                case FishingGame.Difficulty.Medium:
                    AddToBucket(mediumFishValue);
                    State.MediumFishCought++;
                    break;
                case FishingGame.Difficulty.Hard:
                    break;
            }

            Debug.Log($"The player bucketed a fish! The bucket value is now {GetBucketValue()}. Easy caught : " +
                      $"{State.EasyFishCought}, " +
                      $"Medium caught : {State.MediumFishCought}, " +
                      $"Hard caught : {State.HardFishCought}.");
        }

        /// <summary>
        /// This should be called after the player has either won or lost a fishing game.
        /// The purpose is to decrement the durability of the currently equipped bait and revert it to
        /// default if we reach 0.
        /// </summary>
        public void HandleBaitDurability()
        {
            State.EquippedBaitDurability--;
            if (State.EquippedBaitDurability <= 0)
            {
                EquipBait(0, 0);
                baitMenu.ResetToDefaultBait();
            }
        }

        public void EquipBait(int strength, int durability)
        {
            State.EquippedBaitStrength = strength;
            State.EquippedBaitDurability = durability;
        }
    }
}