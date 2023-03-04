using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.HazardManagers;
using SpaceBoat.Ship.Activatables;
using SpaceBoat.UI;
using SpaceBoat.Rewards;
using SpaceBoat.PlayerSubclasses.Equipment;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using TotemEntities.DNA;
using Newtonsoft.Json;


namespace SpaceBoat {
    public enum ActivatablesNames {HarpoonGun, Kitchen, Ladder, Sails, Bedroom, CraftingBench, ShipShield, None};

    public class GameModel : MonoBehaviour
    {
        public static GameModel Instance;

        [Header("Game Settings")]
        [SerializeField] private bool DoNotUpdate = false;
        [SerializeField] private bool playSoundtrack = true;
        [SerializeField] private float musicFadeTime = 1f;
        [SerializeField] private bool slowMo = false;
        [SerializeField] private bool utilityCheats = false;
        [SerializeField] private bool resetSaveFileOnStart = false;
        [SerializeField] public bool unlockEverything = false;
        [SerializeField] public bool skipTutorial = false;
        [SerializeField] private HazardTypes forceHazard = HazardTypes.None;
        

        [Header("Object References")]
        [SerializeField] public Player player;
        [SerializeField] public SoundManager sound;
        [SerializeField] public UI.HelpPromptsManager helpPrompts;
        [SerializeField] public UI.HelpPromptsManager controlsPrompts;
        [SerializeField] public CameraController cameraController;
        [SerializeField] public GameObject theBoat;
        [SerializeField] public CometManager cometManager;
        [SerializeField] public GameObject shipShield;
        [SerializeField] public GameObject bossParent;
       [SerializeField] public GameObject[] heads;

        [Header("Ship")]
        [SerializeField] public List<GameObject> shipSails;
        [SerializeField] public GameObject cometFlightTarget;
        [SerializeField] public GameObject cometDeckTarget;
        [SerializeField] public GameObject HarpoonGun;
        
        // item prefabs
        [Header("Item Prefabs")]
        [SerializeField] public GameObject clothPrefab;
        [SerializeField] public GameObject harpoonPrefab;
        [SerializeField] public GameObject foodPrefab;

        // hazard manager prefabs
        [Header("Hazard Prefabs")] 
        [SerializeField] public List<HazardDefinition> hazardDefinitions;
        [SerializeField] private HazardPlanner hazardPlanner;
        [SerializeField] private float hazardWindDownTime = 15f;

        [Header("Help Prompts && Tutorial")]
        [SerializeField] public Environment.HelpPromptTrigger[] movementTutorialTrigger;
        [SerializeField] public HelpPrompt criticalShipPrompt;
        [SerializeField] public GameObject tutorialHazard;
        [SerializeField] public HelpPrompt tutorialHazardStage1;
        [SerializeField] public HelpPrompt tutorialHazardCamera;
        [SerializeField] public HelpPrompt tutorialHazardStage2;
        [SerializeField] public HelpPrompt tutorialHazardStage3; 
        [SerializeField] public HelpPrompt youreDoingGreat;

        public SaveData saveGame;
        public SaveDataManager saveGameManager;

        public bool movementTutorialPlayed {
            get {return saveGame.movementTutorialPlayed;} 
            set {saveGame.movementTutorialPlayed = value; saveGameManager.Save();}
        }
        public bool cometTutorialPlayed {
            get {return saveGame.cometTutorialPlayed;} 
            set {saveGame.cometTutorialPlayed = value; saveGameManager.Save();}
        }
        public bool craftingTutorialPlayed  {
            get {return saveGame.craftingTutorialPlayed;} 
            set {saveGame.craftingTutorialPlayed = value; saveGameManager.Save();}
        }

        public bool crouchTutorialPlayed {
            get {return saveGame.crouchTutorialPlayed;} 
            set {saveGame.crouchTutorialPlayed = value; saveGameManager.Save();}
        }

        public bool equipmentTutorialPlayed {
            get {return saveGame.equipmentTutorialPlayed;} 
            set {saveGame.equipmentTutorialPlayed = value; saveGameManager.Save();}
        }

        public bool tutorialHazardPlayed {
            get {return saveGame.tutorialHazardPlayed;} 
            set {saveGame.tutorialHazardPlayed = value; saveGameManager.Save();}
        }



        public TotemDNADefaultAvatar playerAvatar { get; private set; }
        public float GameBeganTime {get; private set;}
        public bool gameOverTriggered {get; private set;} = false;
        public int lastSurvivingSailCount {get; private set;}

        private IHazardManager currentHazardManager;
        private Dictionary<HazardTypes, GameObject> hazardManagerPrefabsDict = new Dictionary<HazardTypes, GameObject>();
        private int numHazardsCompleted = 0;
        private float hazardWindDownTimer = 0f;
        public bool hazardWindDown {get; private set;}

        private bool hasRebuiltBuildablesAfterLoad = false;


        public bool isPaused {get; private set;}
        public delegate void PauseEvent();
        private List<PauseEvent> pauseEvents = new List<PauseEvent>();
        private List<PauseEvent> unpauseEvents = new List<PauseEvent>();
        private List<PauseEvent> whilePausedEvents = new List<PauseEvent>();

        public void SetAvatar(TotemDNADefaultAvatar avatar)
        {
            playerAvatar = avatar;
        }

        // Pause
        public void PauseGame() {
            isPaused = true;
            Time.timeScale = 0f;
            foreach (PauseEvent pauseEvent in pauseEvents) {
                pauseEvent();
            }
        }

        public void AddPauseEvent(PauseEvent pauseEvent) {
            pauseEvents.Add(pauseEvent);
        }

        public void UnpauseGame() {
            isPaused = false;
            Time.timeScale = 1f;
            foreach (PauseEvent unpauseEvent in unpauseEvents) {
                unpauseEvent();
            }
        }

        public void AddUnpauseEvent(PauseEvent unpauseEvent) {
            unpauseEvents.Add(unpauseEvent);
        }

        public void AddWhilePausedEvent(PauseEvent whilePausedEvent) {
            whilePausedEvents.Add(whilePausedEvent);
        }



        // activatable management
        public ActivatablesNames GetActivatableType(GameObject activatable) {
            return activatable.GetComponent<IActivatables>().kind;
        }


        public IActivatables GetActivatableComponent(GameObject activatable) {
            return activatable.GetComponent<IActivatables>();
        }

        //animations 
        private List<PlayableGraph> playableGraphs = new List<PlayableGraph>();
        public void PlayAnimation(AnimationClip clip, GameObject target) {
            Animator anim;
            if (target.GetComponent<Animator>() != null) {
                anim = target.GetComponent<Animator>();
            } else {
                anim = target.AddComponent<Animator>();
            }
            if (anim == null) {
                Debug.Log("Null animator for " + target.name);
                return;
            }
            PlayableGraph playableGraph;
            AnimationPlayableUtilities.PlayClip(anim, clip, out playableGraph);
            StartCoroutine(DestroyGraph(playableGraph, clip.length));
        }

        public IEnumerator DestroyGraph(PlayableGraph graph, float delay) {
            yield return new WaitForSeconds(delay);
            graph.Destroy();
        }



        void Awake() {
            // This is a singleton, so if there is already a GameModel in the scene, destroy this one.
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
            }
            Instance = this;

            QualitySettings.vSyncCount = 0;  // VSync must be disabled
            Application.targetFrameRate = 24;

            // Find the playerCharacter 
            if (player == null) {
                Debug.LogError("Player not set in GameModel!");
            }
            if (sound == null) {
                sound = SoundManager.Instance;
                if (sound == null) {
                    sound = FindObjectOfType<SoundManager>();
                    if (sound == null) {
                        Debug.LogError("SoundManager not set in GameModel!");
                    }
                }
            }
            if (helpPrompts == null) {
                Debug.LogError("HelpPromptsManager not set in GameModel!");
            }
            if (cameraController == null) {
                Debug.LogError("CameraController not set in GameModel!");
            }
            saveGameManager = new SaveDataManager();
            if (resetSaveFileOnStart) {
                saveGameManager.Reset();
            } else {
                saveGameManager.Load();
                saveGameManager.ResetBetweenRuns();
            }
            saveGame = saveGameManager.saveData;

            //hazard dictionary
            foreach (HazardDefinition hazardManagerPrefab in hazardDefinitions) {
                hazardManagerPrefabsDict.Add(hazardManagerPrefab.hazardType, hazardManagerPrefab.hazardManagerPrefab);
                if (!saveGame.hazardsCompleted.ContainsKey(hazardManagerPrefab.hazardType)) {
                    saveGame.hazardsCompleted.Add(hazardManagerPrefab.hazardType, false);
                }
            }
            //veryify hazard plans
            foreach (HazardOptions hazardOptions in hazardPlanner.hazardPlan) {
                foreach (HazardTypes hazardType in hazardOptions.hazardOptions) {
                    if (!hazardManagerPrefabsDict.ContainsKey(hazardType)) {
                        Debug.LogError("Hazard type " + hazardType + " not found in hazard manager dictionary, but is used in a hazard plan!");
                    }
                }
            }


            GameBeganTime = Time.time;
            lastSurvivingSailCount = shipSails.Count;
        }

        public void Start() {
            Debug.Log("Game is starting!");

            if (slowMo) {
                Time.timeScale = 0.1f;
            }

            sound.Play("Spawn");
            if (sound.IsPlaying("MenuSoundtrack")) {
                sound.Stop("MenuSoundtrack");
            }
            if (playSoundtrack) sound.Play("Interlude", 1f, true, musicFadeTime);
            if (tutorialHazardPlayed) {
                foreach (Environment.HelpPromptTrigger trigger in movementTutorialTrigger) {
                    trigger.gameObject.SetActive(false);
                }
            }


            if (DoNotUpdate) return;

        }

        private IEnumerator ToBeContinued() {
            yield return new WaitForSeconds(3f);
            SceneManager.LoadScene("ToBeContinued");
        }

        public void TriggerToBeContinued() {
            StartCoroutine(ToBeContinued());
        }


        private IEnumerator GameOver(float delay = 2f) {
            Debug.Log("Gameover!");
            saveGameManager.ResetBetweenRuns();
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene("GameOver");
        }

        public void TriggerGameOver(float delay = 2f) {
            StartCoroutine(GameOver(delay));
            gameOverTriggered = true;
        }

        public List<GameObject> SelectSailsForTargetting(int maxNumSelect) {
            List<GameObject> targetSails = new List<GameObject>();
            List<SailsActivatable> availableSails = new List<SailsActivatable>();
            bool respectCooldown = GameModel.Instance.lastSurvivingSailCount > maxNumSelect;
            Debug.Log("Selecting max " + maxNumSelect + " sails to break.");
            foreach (GameObject sail in this.shipSails)
            {
                SailsActivatable sailScript = sail.GetComponent<SailsActivatable>();
                if (!sailScript.isBroken)
                {
                    availableSails.Add(sailScript);
                }
            }
            Debug.Log("Found " + availableSails.Count + " valid sails to break.");
            // sort in order of last targetted time
            availableSails.Sort((a, b) => a.lastTargettedTime.CompareTo(b.lastTargettedTime));
            // add one sail from each grouping if available.
            Dictionary<SailsActivatable.SailGrouping, bool> groupings = new Dictionary<SailsActivatable.SailGrouping, bool>();
            foreach (SailsActivatable sail in availableSails) {
                if (!groupings.ContainsKey(sail.sailGrouping)) {
                    Debug.Log("Targetting sail " + sail.name + " from grouping " + sail.sailGrouping + ", last targetted at " + sail.lastTargettedTime + ".");
                    groupings.Add(sail.sailGrouping, true);
                    targetSails.Add(sail.gameObject);
                    if (targetSails.Count >= maxNumSelect) {
                        return targetSails;
                    }
                }
            }
            Debug.Log("Targetting " + (maxNumSelect - targetSails.Count) + " additional sails ignoring groupings.");
            // add remaining sails if necessary;
            foreach (SailsActivatable sail in availableSails) {
                if (!targetSails.Contains(sail.gameObject)) {
                    targetSails.Add(sail.gameObject);
                    if (targetSails.Count >= maxNumSelect) {
                        break;
                    }
                }
            }
            return targetSails;
        }

        void DestroyShip() {
            cameraController.ForceCameraBehaviour(true, -24, 13.5f, 40);
            Rigidbody2D shipRigidbody = theBoat.GetComponent<Rigidbody2D>();
            Rigidbody2D[] subRigidbodies = theBoat.GetComponentsInChildren<Rigidbody2D>();
            shipRigidbody.bodyType = RigidbodyType2D.Dynamic;
            shipRigidbody.gravityScale = 0.3f;
            shipRigidbody.AddForce(new Vector2(-80, 0));
            shipRigidbody.AddTorque(0.15f);
            Environment.ShipWheel wheel = theBoat.GetComponentInChildren<Environment.ShipWheel>();
            wheel.enabled = false;
            foreach (Rigidbody2D subRigidbody in subRigidbodies) {
                subRigidbody.bodyType = RigidbodyType2D.Dynamic;
                subRigidbody.gravityScale = 0.3f;
                subRigidbody.AddForce(new Vector2(Random.Range(-90,-70), 0));
                subRigidbody.AddTorque(0.15f);
            }
            Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
            playerBody.velocity = new Vector2(0, 0);
            playerBody.bodyType = RigidbodyType2D.Dynamic;
            playerBody.gravityScale = 0.3f;
            playerBody.AddForce(new Vector2(-85, 0));
            Destroy(player);

        }

        (GameObject, HazardDifficulty) PickNextHazard() {
            if (forceHazard != HazardTypes.None) {
                HazardTypes hazardType = forceHazard;
                forceHazard = HazardTypes.None;
                Debug.Log("Force Spawned hazard " + hazardType + " with difficulty " + HazardDifficulty.Easy + ".");
                return (hazardManagerPrefabsDict[hazardType], HazardDifficulty.Easy);
            }

            if (numHazardsCompleted >= hazardPlanner.hazardPlan.Count) {
                Debug.Log("No more hazards to spawn, game over!");
                TriggerToBeContinued();
                return (null, HazardDifficulty.Easy);;
            }
            HazardOptions hazardOptions = hazardPlanner.hazardPlan[numHazardsCompleted];
            List<HazardTypes> validHazards = new List<HazardTypes>();
            foreach (HazardTypes hazardType in hazardOptions.hazardOptions) {
                if (!saveGame.hazardsCompleted[hazardType]) {
                    validHazards.Add(hazardType);
                }
            }
            if (validHazards.Count == 0) {
                Debug.LogError("All of the hazards registered in Hazard Plan " + numHazardsCompleted + " are already complete! Mistake in setup in the Game Model?");
                TriggerToBeContinued();
                return (null, HazardDifficulty.Easy);
            }
            HazardTypes hazardTypeToSpawn = validHazards[Random.Range(0, validHazards.Count)];
            Debug.Log("Spawning hazard " + hazardTypeToSpawn + " with difficulty " + hazardOptions.difficulty + ".");
            return (hazardManagerPrefabsDict[hazardTypeToSpawn], hazardOptions.difficulty);
        }


        void CheckHazardProgress() {
            if (hazardWindDownTimer > 0) {
                hazardWindDownTimer = Mathf.Max(0, hazardWindDownTimer - Time.deltaTime);
                return;
            } else if (hazardWindDown) {
                Debug.Log("Hazard wind-down complete, starting new hazard");
                hazardWindDown = false;
            }
            if (currentHazardManager != null && currentHazardManager.HasEnded) {
                Debug.Log("Hazard has ended, starting wind-down timer");
                saveGame.hazardsCompleted[currentHazardManager.HazardType] = true;
                if (currentHazardManager.HazardSoundtrack != "" && sound.IsPlaying(currentHazardManager.HazardSoundtrack)) {
                    sound.Stop(currentHazardManager.HazardSoundtrack, true, 1f);
                    sound.Play("Interlude", 1f, true,  0.5f, 0.5f);
                }
                Destroy(currentHazardManager.gameObject);
                currentHazardManager = null;
                numHazardsCompleted++;
                hazardWindDownTimer = hazardWindDownTime;
                hazardWindDown = true;
                cometManager.StartCometBurst();
                return;
            }
            if (currentHazardManager == null) {
                //new hazard or enemy.
                if (numHazardsCompleted == 0) {
                    if (!CheckMoveTutorialComplete()) return;
                    else {
                        Debug.Log("Tutorial complete, starting first Hazard");
                        cometManager.StartCometSpawner();
                    }
                }
                (GameObject nextHazard, HazardDifficulty difficulty) = PickNextHazard();
                if (nextHazard == null) return;
                GameObject newHazard = Instantiate(nextHazard, new Vector3(0, 0, 0), Quaternion.identity);
                Debug.Log("New hazard: " + newHazard.name);
                currentHazardManager = newHazard.GetComponent<IHazardManager>();
                currentHazardManager.StartHazard(difficulty);
                if (playSoundtrack && currentHazardManager.HazardSoundtrack != "") {
                    sound.Stop("Interlude", true, musicFadeTime);
                    sound.Play(currentHazardManager.HazardSoundtrack,1f, true, musicFadeTime, musicFadeTime);
                }
            }

        }

        private bool tutorialHazardTriggered = false;
        bool CheckMoveTutorialComplete() {
            if (skipTutorial) return true;
            if (tutorialHazardPlayed) return true;
            if (helpPrompts.wasPromptDisplayed("MovementTutorial", true) && !tutorialHazardTriggered) {
                movementTutorialPlayed = true;
                StartTutorialHazard();
                tutorialHazardTriggered = true;
            }
            return false;
        }

        IEnumerator TutorialHazard() {
            GameObject newHazard = Instantiate(tutorialHazard, new Vector3(0, 0, 0), Quaternion.identity);
            MeteorShower shower = newHazard.GetComponent<MeteorShower>();
            shower.StartHazard(HazardDifficulty.Easy);
            int tutorialSailsRepaired = 0;
            bool stage1 = false;
            bool stage2 = false;
            bool stage3 = false;
            int numMeteorsHit = 0;
            int numMeteorsLastPrompt = 0;
            foreach (GameObject sail in shipSails) {
                SailsActivatable sailActivatable = sail.GetComponent<SailsActivatable>();
                sailActivatable.AddOnSailRepairCallback(() => {
                    if (shower != null) {
                        tutorialSailsRepaired++;
                        numMeteorsLastPrompt = numMeteorsHit;
                    }
                });
            }
            while (shower.meteorsOut == 0) {
                yield return null;
            }
            Debug.Log("Tutorial hazard fired first meteor");
            yield return new WaitForSeconds(10.5f);
            cameraController.AddShipViewOverride("HazardStartup", 1);  
            while (shower.meteorsOut > 0) {
                yield return null;
            }
            helpPrompts.AddPrompt(tutorialHazardStage1);
            yield return new WaitForSeconds(1f);
            cameraController.RemoveShipViewOverride("HazardStartup");   

            while (!stage1) {
                if (tutorialSailsRepaired >= 1) {
                    stage1 = true;
                }
                if (numMeteorsHit - numMeteorsLastPrompt >= 2) {
                    numMeteorsLastPrompt = numMeteorsHit;
                    helpPrompts.AddPrompt(tutorialHazardStage1);
                }
                if (shower.meteorsOut > 0) {
                    while (shower.meteorsOut > 0) {
                        yield return null;
                    }
                    numMeteorsHit++;
                }
                yield return null;
            }
            shower.tutorialStage = 1;
            yield return new WaitForSeconds(3f);
            cameraController.AddShipViewOverride("HazardStartup", 1);  
            helpPrompts.AddPrompt(tutorialHazardStage2);
            yield return new WaitForSeconds(3f);
            cameraController.RemoveShipViewOverride("HazardStartup");   
            float stage2Timer = 0;
            while (!stage2) {
                stage2Timer += Time.deltaTime;
                if (stage2Timer >= 24) {
                    shower.tutorialStage = 2;
                }
                if (tutorialSailsRepaired >= 4) {
                    stage2 = true;
                } else if (tutorialSailsRepaired >= 2) {
                    helpPrompts.AddPrompt(tutorialHazardCamera);
                }
                if (numMeteorsHit - numMeteorsLastPrompt >= 2) {
                    numMeteorsLastPrompt = numMeteorsHit;
                    helpPrompts.AddPrompt(tutorialHazardStage1);
                }
                yield return null;
            }
            shower.tutorialStage = 2;            
            GameObject comet = cometManager.SpawnComet(4, 100);
            comet.GetComponent<RewardComet>().AddCometShatterCallback(() => {
                stage3 = true;
            });
            yield return new WaitForSeconds(3f);
            helpPrompts.AddPrompt(tutorialHazardStage3);        
            cameraController.AddShipViewOverride("HazardStartup", 1); 
            yield return new WaitForSeconds(3f);
            int numCometsSpawned = 1;
            float stage3Timer = 0;
            GameModel.Instance.HarpoonGun.GetComponentInChildren<HarpoonGunActivatable>().supressPromptDuringTutorial = false;
            while (!stage3) {
                Debug.Log("Stage 3 timer: " + stage3Timer);
                Debug.Log("Num comets spawned: " + numCometsSpawned);
                if (stage3Timer > numCometsSpawned*10) {
                    GameObject newComet = cometManager.SpawnComet(4, 100);
                    newComet.GetComponent<RewardComet>().AddCometShatterCallback(() => {
                        stage3 = true;
                    });
                    numCometsSpawned++;
                    if (numCometsSpawned == 3 || numCometsSpawned > 5) {
                        helpPrompts.AddPrompt(tutorialHazardStage3);
                    }
                }
                stage3Timer += Time.deltaTime;
                yield return null;
            }

            cameraController.RemoveShipViewOverride("HazardStartup");  
            Destroy(shower.gameObject);
            tutorialHazardPlayed = true;
            yield return new WaitForSeconds(3f);
            helpPrompts.AddPrompt(youreDoingGreat);
            yield break;
        }
        
        void StartTutorialHazard() {
            StartCoroutine(TutorialHazard());            
        }

        // check if any sails remain unbroken
        // trigger gameover if none remain
        public void Update() {
            int currentFrame = Time.frameCount;
            if (currentFrame%24 == 0) {
                //Debug.Log("Frame " + currentFrame + ", Time " + Time.time);
            }

            if (utilityCheats) {
                if (Input.GetKeyDown(KeyCode.P)) {
                    foreach (GameObject sail in shipSails) {
                        if (sail.GetComponent<Ship.Activatables.SailsActivatable>().isBroken == false) {
                            sail.GetComponent<Ship.Activatables.SailsActivatable>().Break();
                            break;
                        }
                    }
                }
            }
            if (isPaused) {
                foreach (PauseEvent pauseEvent in whilePausedEvents) {
                    pauseEvent();
                }
            }


            if (DoNotUpdate) return;
            int num_surviving_sails = 0;
            foreach (GameObject sail in shipSails) {
                 if (sail.GetComponent<Ship.Activatables.SailsActivatable>().isBroken == false) {
                    num_surviving_sails++;
                }
            }
            if (num_surviving_sails == 0 && !gameOverTriggered) {
                DestroyShip();
                TriggerGameOver(5);
            } else if (num_surviving_sails <= 2) {
                if (!sound.IsPlaying("ShipLowHP")) {
                    sound.Play("ShipLowHP");
                }
                helpPrompts.AddPrompt(criticalShipPrompt, () => {
                     int num_surviving_sails = 0;
                    foreach (GameObject sail in shipSails) {
                        if (sail.GetComponent<Ship.Activatables.SailsActivatable>().isBroken == false) {
                            num_surviving_sails++;
                        }
                    }
                    return (num_surviving_sails > 2);
                });
            } else if (num_surviving_sails > 1) {
                if (sound.IsPlaying("ShipLowHP")) {
                    sound.Stop("ShipLowHP");
                }
            }
            lastSurvivingSailCount = num_surviving_sails;

            CheckHazardProgress();
        }

        // save player progress
        public void SaveForQuit() {
            SavePersistantInfo();
        }

        public void SavePersistantInfo() {

        }


        // subclasses for saving and loading
        [System.Serializable] public class SaveData {
            public int money = 0;
            public Dictionary<RewardType, bool> rewardsUnlocked = new Dictionary<Rewards.RewardType, bool>() {
                {RewardType.DashEquipmentBlueprint, false},
                {RewardType.HarpoonGunBuildableBlueprint, false},
                {RewardType.HarpoonLauncherEquipmentBlueprint, false},
                {RewardType.ShieldEquipmentBlueprint, false},
                {RewardType.HealthPackEquipmentBlueprint, false},
                {RewardType.JumpPadBuildableBlueprint, false},
                {RewardType.ShipShieldBuildableBlueprint, false}
            };

            public Dictionary<EquipmentType, bool> equipmentBuilt = new Dictionary<EquipmentType, bool>() {
                {EquipmentType.Dash, false},
                {EquipmentType.HarpoonLauncher, false},
                {EquipmentType.Shield, false},
                {EquipmentType.HealthPack, false}
            };

            public List<Ship.Buildables.buildableSaveData> buildables = new List<Ship.Buildables.buildableSaveData>();

            public Dictionary<HazardTypes, bool> hazardsCompleted = new Dictionary<HazardTypes, bool>();

            public bool movementTutorialPlayed;
            public bool cometTutorialPlayed;
            public bool craftingTutorialPlayed;
            public bool equipmentTutorialPlayed;
            public bool crouchTutorialPlayed;
            public bool tutorialHazardPlayed;
        }

        public class SaveDataManager {

            public SaveData saveData { get; private set; }
            private static string saveDataPath = Application.persistentDataPath + "/SpaceBoatSave.json";
            
            public SaveDataManager() {
                saveData = new SaveData();
            }

            public void Reset() {
                saveData = new SaveData();
                Debug.Log("Reset save data - " + saveDataPath);
                Save();
            }

            public void ResetBetweenRuns() {
                saveData.equipmentBuilt = new Dictionary<EquipmentType, bool> {
                    {EquipmentType.Dash, false},
                    {EquipmentType.HarpoonLauncher, false},
                    {EquipmentType.Shield, false},
                    {EquipmentType.HealthPack, false}
                };
                saveData.hazardsCompleted.Clear();
                saveData.money = 0;
                Save();
            }

            public void Save() {
                using (StreamWriter writer = new StreamWriter(saveDataPath)) {
                    string data = JsonConvert.SerializeObject(saveData);
                    writer.Write(data);
                }
            }

            public void Load() {
                if (File.Exists(saveDataPath)) {
                    using (StreamReader reader = new StreamReader(saveDataPath)) {
                        string data = reader.ReadToEnd();
                        saveData = JsonConvert.DeserializeObject<SaveData>(data);
                    }
                }
            }
        }
    }

    

    public class EventSystem {
        public enum EventName {
            OnPlayerDamage,
            OnPlayerDeath,
            OnPlayerHeal,
            OnPlayerJumps,
            OnPlayerLands,
            OnPlayerStateChange,
            OnActivatableActivate,
            OnActivatableDeactivate,
            OnActivatableZoneEnter,
            OnActivatableZoneExit,
            OnSaiActivatableBroken,
            OnSailActivatableRepaired,
            OnCraftBlueprintFound,
            OnCraftBlueprintCrafted,
            OnEquipmentUnlock,
            OnEquipmentEquip,
            OnEquipmentUnequip,
            OnBuildableStart,
            OnBuildableConfirm,
            OnBuildableCancel,
            OnHazardBegin,
            OnHazardEnd,
        }
        public delegate bool EventCondition(EventContext context);
        public delegate void EventCallback(EventContext context);

        private struct EventListener {
            public string name;
            public EventName eventName;
            public EventCondition condition;
            public EventCallback callback;
            public bool persistListener;
        }

        private Dictionary<string, List<EventListener>> eventListenersByKey = new Dictionary<string, List<EventListener>>();
        private Dictionary<EventName, List<string>> listenerKeysByEvent = new Dictionary<EventName, List<string>>();

        public void AddListener(string name, EventName eventName, EventCondition condition, EventCallback callback, bool persistListener = false) {
            EventListener listener = new EventListener();
            listener.name = name;
            listener.eventName = eventName;
            listener.condition = condition;
            listener.callback = callback;
            listener.persistListener = persistListener;
            if (!eventListenersByKey.ContainsKey(name)) {
                eventListenersByKey.Add(name, new List<EventListener>());
            }
            eventListenersByKey[name].Add(listener);
            if (!listenerKeysByEvent[eventName].Contains(name)) {
                listenerKeysByEvent[eventName].Add(name);
            }
        }

        public void AddListener(string name, EventName eventName, bool conditionReturn, EventCallback callback, bool persistListener = false) {
            if (conditionReturn == false) return;
            AddListener(name, eventName, (context) => { return conditionReturn; }, callback, persistListener);
        }

        public void RemoveListener(string name) {
            List<EventListener> listeners = eventListenersByKey[name];
            List<EventName> eventNames = new List<EventName>();
            foreach (EventListener listener in listeners) {
                eventNames.Add(listener.eventName);
            }
            foreach (EventName eventName in eventNames) {
                listenerKeysByEvent[eventName].Remove(name);
            }
            eventListenersByKey[name].Clear();
        }

        void TriggerEvent(EventName eventName, EventContext context) {
            List<string> listenerKeys = listenerKeysByEvent[eventName];
            Debug.Log("Triggering event: " + eventName + " with " + listenerKeys.Count + " listeners");
            foreach (string listenerKey in listenerKeys) {
                List<EventListener> listeners = eventListenersByKey[listenerKey];
                foreach (EventListener listener in listeners) {
                    if (listener.eventName == eventName && listener.condition(context)) {
                        listener.callback(context);
                    }
                }
            }
        }

        public void TriggerEvent(EventName eventName, params object[] args) {
            EventContext context = new EventContext(args);
            TriggerEvent(eventName, context);
        }

        EventSystem() {
            foreach (EventName eventName in System.Enum.GetValues(typeof(EventName))) {
                listenerKeysByEvent.Add(eventName, new List<string>());
            }
        }

        public class EventContext {
            private Player player;
            public Player Player {get {return player;}}
            private PlayerStateName playerState;
            public PlayerStateName PlayerState {get {return playerState;}}
            private IActivatables activatable;
            public IActivatables Activatable {get {return activatable;}}
            private Rewards.ICraftBlueprint craftBlueprint;
            public Rewards.ICraftBlueprint CraftBlueprint {get {return craftBlueprint;}}
            private IPlayerEquipment equipment;
            public IPlayerEquipment Equipment {get {return equipment;}}
            private Ship.Buildables.IBuildable buildable;
            public Ship.Buildables.IBuildable Buildable {get {return buildable;}}

            public EventContext(params object[] args) {
                foreach (object arg in args) {
                    if (arg is Player) {
                        player = (Player)arg;
                    } else if (arg is PlayerStateName) {
                        playerState = (PlayerStateName)arg;
                    } else if (arg is IActivatables) {
                        activatable = (IActivatables)arg;
                    } else if (arg is Rewards.ICraftBlueprint) {
                        craftBlueprint = (Rewards.ICraftBlueprint)arg;
                    } else if (arg is IPlayerEquipment) {
                        equipment = (IPlayerEquipment)arg;
                    } else if (arg is Ship.Buildables.IBuildable) {
                        buildable = (Ship.Buildables.IBuildable)arg;
                    }
                }
            }
        }
    }
}