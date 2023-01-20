using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.HazardManagers;
using SpaceBoat.Ship;
using SpaceBoat.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;


namespace SpaceBoat {

    public enum ActivatablesNames {HarpoonGun, Kitchen, Ladder, Sails, None};

    public class GameModel : MonoBehaviour
    {
        public static GameModel Instance;

        [Header("Game Settings")]
        [SerializeField] private bool environmentTesting = false;
        [SerializeField] private bool playSoundtrack = true;
        [SerializeField] private bool slowMo = false;

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
        [Header("Hazard Manager Prefabs")]
        [SerializeField] public GameObject meteorManagerPrefab;

        [Header("Enemy Prefabs")] 
        [SerializeField] public GameObject hydraPrefab;

        [Header("Help Prompts")]
        [SerializeField] public HelpPrompt criticalShipPrompt;


        public Player player {get; private set;}
        public SoundManager sound {get; private set;}

        public UI.HelpPromptsManager helpPrompts {get; private set;}

        public float GameBeganTime {get; private set;}
        public int lastSurvivingSailCount {get; private set;}

        private IHazardManager currentHazardManager;

        public bool isPaused {get; private set;}
        public delegate void PauseEvent();
        private List<PauseEvent> pauseEvents = new List<PauseEvent>();
        private List<PauseEvent> unpauseEvents = new List<PauseEvent>();

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

    

        //hazard management
        public IHazardManager CreateHazardManager(string hazardManagerType) {
            if (hazardManagerType == "MeteorShower") {
                return Instantiate(meteorManagerPrefab).GetComponent<MeteorShower>();
            }
            return null;
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
            if (FindObjectsOfType<GameModel>().Length > 1) {
                Destroy(gameObject);
            }
            Instance = this;

            QualitySettings.vSyncCount = 0;  // VSync must be disabled
            Application.targetFrameRate = 24;

            // Find the playerCharacter 
            player = FindObjectOfType<Player>();
            helpPrompts = FindObjectOfType<UI.HelpPromptsManager>();

            GameBeganTime = Time.time;
            lastSurvivingSailCount = shipSails.Count;
        }

        public void Start() {
            Debug.Log("Game is starting!");
            
            sound = SoundManager.Instance;

            if (slowMo) {
                Time.timeScale = 0.1f;
            }

            sound.Play("Spawn");
            if (sound.IsPlaying("MenuSoundtrack")) {
                sound.Stop("MenuSoundtrack");
            }
            if (playSoundtrack) sound.Play("GameplaySoundtrack");
            if (environmentTesting) return;
            //TODO add random hazard selection.
            currentHazardManager = CreateHazardManager("MeteorShower");
            
            currentHazardManager?.StartHazard();

        }

        private IEnumerator ToBeContinued() {
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene("ToBeContinued");
        }

        public void TriggerToBeContinued() {
            StartCoroutine(ToBeContinued());
        }


        private IEnumerator GameOver() {
            Debug.Log("Gameover!");
            yield return new WaitForSeconds(2);
            //SoundManager.Instance.Stop("GameplaySoundtrack");
            SceneManager.LoadScene("GameOver");
        }

        public void TriggerGameOver() {
            StartCoroutine(GameOver());
        }



        void CheckHazardProgress() {
            if (currentHazardManager == null || currentHazardManager.hasEnded) {
                if (currentHazardManager != null) {
                    Destroy(currentHazardManager.gameObject);
                    currentHazardManager = null;
                }
                //new hazard or enemy.

            }
        }

        // check if any sails remain unbroken
        // trigger gameover if none remain
        public void Update() {
            if (environmentTesting) return;
            int num_surviving_sails = 0;
            foreach (GameObject sail in shipSails) {
                 if (sail.GetComponent<Ship.SailsActivatable>().isBroken == false) {
                    num_surviving_sails++;
                }
            }
            if (num_surviving_sails == 0) {
                TriggerGameOver();
            } else if (num_surviving_sails == 1) {
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
                    return (num_surviving_sails > 1);
                });
            } else if (num_surviving_sails > 1) {
                if (sound.IsPlaying("ShipLowHP")) {
                    sound.Stop("ShipLowHP");
                }
            }
            lastSurvivingSailCount = num_surviving_sails;

            CheckHazardProgress();
        }
    }
}