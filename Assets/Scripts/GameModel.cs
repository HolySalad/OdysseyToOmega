using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.HazardManagers;
using SpaceBoat.Ship;
using SpaceBoat.UI;
using SpaceBoat.Rewards;
using SpaceBoat.PlayerSubclasses.Equipment;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;
using TotemEntities.DNA;
using Newtonsoft.Json;


namespace SpaceBoat {
    public enum ActivatablesNames {HarpoonGun, Kitchen, Ladder, Sails, Bedroom, CraftingBench, None};

    public class GameModel : MonoBehaviour
    {
        public static GameModel Instance;

        [Header("Game Settings")]
        [SerializeField] private bool DoNotUpdate = false;
        [SerializeField] private bool playSoundtrack = true;
        [SerializeField] private bool slowMo = false;
        [SerializeField] private bool utilityCheats = false;
        [SerializeField] private bool resetSaveFileOnStart = false;

        [Header("Object References")]
        [SerializeField] public Player player;
        [SerializeField] public SoundManager sound;
        [SerializeField] public UI.HelpPromptsManager helpPrompts;
        [SerializeField] public CameraController cameraController;
        [SerializeField] public GameObject theBoat;
        [SerializeField] public CometManager cometManager;

        [Header("Ship")]
        [SerializeField] public List<GameObject> shipSails;
        [SerializeField] public GameObject cometFlightTarget;
        [SerializeField] public GameObject cometDeckTarget;
        
        // item prefabs
        [Header("Item Prefabs")]
        [SerializeField] public GameObject clothPrefab;
        [SerializeField] public GameObject harpoonPrefab;
        [SerializeField] public GameObject foodPrefab;

        // hazard manager prefabs
        [SerializeField] public List<GameObject> hazardManagerPrefabs;
        [SerializeField] private float hazardWindDownTime = 15f;

        [Header("Enemy Prefabs")] 
        [SerializeField] public GameObject hydraPrefab;

        [Header("Help Prompts && Tutorial")]
        [SerializeField] public Environment.HelpPromptTrigger[] movementTutorialTrigger;
        [SerializeField] public HelpPrompt criticalShipPrompt;

        public SaveData saveGame;
        public SaveDataManager saveGameManager;

        public bool movementTutorialPlayed {
            get {return saveGame.movementTutorialPlayed;} 
            private set {saveGame.movementTutorialPlayed = value; saveGameManager.Save();}
        }
        public bool cometTutorialPlayed {
            get {return saveGame.cometTutorialPlayed;} 
            private set {saveGame.cometTutorialPlayed = value; saveGameManager.Save();}
        }
        public bool craftingTutorialPlayed  {
            get {return saveGame.craftingTutorialPlayed;} 
            private set {saveGame.craftingTutorialPlayed = value; saveGameManager.Save();}
        }



        public TotemDNADefaultAvatar playerAvatar { get; private set; }
        public float GameBeganTime {get; private set;}
        public bool gameOverTriggered {get; private set;}
        public int lastSurvivingSailCount {get; private set;}

        private IHazardManager currentHazardManager;
        private int hazardsCompleted = 0;
        private float hazardWindDownTimer = 0f;
        public bool hazardWindDown {get; private set;}


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
            }
            saveGame = saveGameManager.saveData;

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
            //if (playSoundtrack) sound.Play("GameplaySoundtrack");
            if (movementTutorialPlayed) {
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
            yield return new WaitForSeconds(delay);
            //SoundManager.Instance.Stop("GameplaySoundtrack");
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

        GameObject PickNextHazard() {
            List<GameObject> availableHazards = new List<GameObject>();
            int highestPriority = -1;
            Debug.Log("Selecting one of " + hazardManagerPrefabs.Count + " hazards for the next hazard");
            foreach (GameObject hazard in hazardManagerPrefabs) {
                IHazardManager hazardManager = hazard.GetComponent<IHazardManager>();
                if (!hazardManager.wasCompleted && hazardsCompleted >= hazardManager.GetEarliestAppearence() && hazardsCompleted < hazardManager.GetLatestAppearence()) {
                    availableHazards.Add(hazard);
                    if (hazardManager.GetPriority() > highestPriority) {
                        highestPriority = hazardManager.GetPriority();
                    }
                }
            }
            if (availableHazards.Count == 0) {
                Debug.Log("No hazards available, game has ended.");
                TriggerToBeContinued();
                return null;
            }
            List<GameObject> highestPriorityHazards = new List<GameObject>();
            foreach (GameObject hazard in availableHazards) {
                IHazardManager hazardManager = hazard.GetComponent<IHazardManager>();
                if (hazardManager.GetPriority() == highestPriority) {
                    highestPriorityHazards.Add(hazard);
                }
            }
            return highestPriorityHazards[Random.Range(0, highestPriorityHazards.Count)];
        }


        void CheckHazardProgress() {
            if (hazardWindDownTimer > 0) {
                hazardWindDownTimer = Mathf.Max(0, hazardWindDownTimer - Time.deltaTime);
                return;
            } else if (hazardWindDown) {
                Debug.Log("Hazard wind-down complete, starting new hazard");
                hazardWindDown = false;
            }
            if (currentHazardManager != null && currentHazardManager.hasEnded) {
                if (hazardWindDownTimer == 0) {
                    Debug.Log("Hazard has ended, starting wind-down timer");
                    hazardManagerPrefabs.Remove(currentHazardManager.gameObject);
                    Destroy(currentHazardManager.gameObject);
                    currentHazardManager = null;
                    hazardsCompleted++;
                    hazardWindDownTimer = hazardWindDownTime;
                    hazardWindDown = true;
                    cometManager.StartCometBurst();
                    return;
                }
            }
            if (currentHazardManager == null) {
                //new hazard or enemy.
                if (hazardsCompleted == 0) {
                    if (!CheckMoveTutorialComplete()) return;
                    else {
                        Debug.Log("Tutorial complete, starting first Hazard");
                        cometManager.StartCometSpawner();
                    }
                }
                GameObject nextHazard = PickNextHazard();
                if (nextHazard == null) return;
                GameObject newHazard = Instantiate(nextHazard, new Vector3(0, 0, 0), Quaternion.identity);
                Debug.Log("New hazard: " + newHazard.name);
                currentHazardManager = newHazard.GetComponent<IHazardManager>();
                currentHazardManager.StartHazard();
                if (playSoundtrack && currentHazardManager.hazardSoundtrack != "") {
                    sound.Play(currentHazardManager.hazardSoundtrack);
                }
            }

        }

        bool CheckMoveTutorialComplete() {
            if (movementTutorialPlayed) return true;
            if (helpPrompts.wasPromptDisplayed("MovementTutorial", true) && helpPrompts.wasPromptDisplayed("JumpTutorial", true)) {
                movementTutorialPlayed = true;
                return true;
            }
            return false;
        }

        // check if any sails remain unbroken
        // trigger gameover if none remain
        public void Update() {

            if (utilityCheats) {
                if (Input.GetKeyDown(KeyCode.P)) {
                    foreach (GameObject sail in shipSails) {
                        if (sail.GetComponent<Ship.SailsActivatable>().isBroken == false) {
                            sail.GetComponent<Ship.SailsActivatable>().Break();
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
                 if (sail.GetComponent<Ship.SailsActivatable>().isBroken == false) {
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
                        if (sail.GetComponent<Ship.SailsActivatable>().isBroken == false) {
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
            public int money = 1000;
            public Dictionary<RewardType, bool> rewardsUnlocked = new Dictionary<Rewards.RewardType, bool>() {
                {RewardType.DashEquipmentBlueprint, true},
                {RewardType.HarpoonGunActivatableBlueprint, false},
                {RewardType.HarpoonLauncherEquipmentBlueprint, false},
                {RewardType.ShieldEquipmentBlueprint, true},
                {RewardType.HealthPackEquipmentBlueprint, false},
                {RewardType.TrampolineActivatableBlueprint, false},
                {RewardType.ShipShieldActivatableBlueprint, false}
            };

            public Dictionary<EquipmentType, bool> equipmentBuilt = new Dictionary<EquipmentType, bool>() {
                {EquipmentType.Dash, false},
                {EquipmentType.HarpoonLauncher, false},
                {EquipmentType.Shield, false},
                {EquipmentType.HealthPack, false}
            };

            public bool movementTutorialPlayed;
            public bool cometTutorialPlayed;
            public bool craftingTutorialPlayed;
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
                saveData.equipmentBuilt.Clear();
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
}