using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;
using SpaceBoat.HazardManagers;
using SpaceBoat.Ship;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;


namespace SpaceBoat {

    public enum ItemTypes {ClothItem, FoodItem, HarpoonItem, None};
    public enum Activatables {HarpoonGun, None};

    public enum HazardProjectiles {Meteor, SpacRock, None}

    public class GameModel : MonoBehaviour
    {
        public static GameModel Instance;

        [Header("Game Settings")]
        [SerializeField] private bool environmentTesting = false;
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


        public Player player {get; private set;}
        public SoundManager sound {get; private set;}

        public UI.HelpPrompts helpPrompts {get; private set;}

        public float GameBeganTime {get; private set;}

        private IHazardManager currentHazardManager;


    
        //item management

        public ItemTypes GetItemType(GameObject item) {
            if (item.GetComponent<ClothItem>() != null) {
                return ItemTypes.ClothItem;
            } else if (item.GetComponent<FoodItem>() != null) {
                return ItemTypes.FoodItem;
            } else if (item.GetComponent<HarpoonItem>() != null) {
                return ItemTypes.HarpoonItem;
            } else {
                return ItemTypes.None;
            }
        }

        public IHeldItems CreateItemComponent(GameObject target, ItemTypes itemType) {
            if (itemType == ItemTypes.ClothItem) {
                return target.AddComponent<ClothItem>();
            } else if (itemType == ItemTypes.HarpoonItem) {
                return target.AddComponent<HarpoonItem>();
            } else if (itemType == ItemTypes.FoodItem) {
                return target.AddComponent<FoodItem>();
            }
            Debug.Log("Unreigstered item type "+ itemType.ToString());
            return null;
        }


        public GameObject PrefabForItemType(ItemTypes itemType) {
            if (itemType == ItemTypes.ClothItem) {
                return clothPrefab;
            } else if (itemType == ItemTypes.HarpoonItem) {
                return harpoonPrefab;
            } else if (itemType == ItemTypes.FoodItem) {
                return foodPrefab;
            }
            Debug.Log("Unregistered item type " + itemType.ToString());
            return null;
        }

        //hazard management
        public IHazardManager CreateHazardManager(string hazardManagerType) {
            if (hazardManagerType == "MeteorShower") {
                return Instantiate(meteorManagerPrefab).GetComponent<MeteorShower>();
            }
            return null;
        }

        // activatable management
        public Activatables GetActivatableType(GameObject activatable) {
            if (activatable.GetComponent<Ship.HarpoonGun>() != null) {
                return Activatables.HarpoonGun;
            } else {
                return Activatables.None;
            }
        }

        
        public IActivatables GetActivatableComponent(GameObject activatable) {
            if (activatable.GetComponent<Ship.HarpoonGun>() != null) {
                return activatable.GetComponent<Ship.HarpoonGun>();
            } else {
                return null;
            }
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
            helpPrompts = FindObjectOfType<UI.HelpPrompts>();

            GameBeganTime = Time.time;
        }

        public void Start() {
            Debug.Log("Game is starting!");
            
            sound = SoundManager.Instance;

            if (slowMo) {
                Time.timeScale = 0.1f;
            }


            if (environmentTesting) return;
            sound.Play("Spawn");
            if (sound.IsPlaying("MenuSoundtrack")) {
                sound.Stop("MenuSoundtrack");
            }
            sound.Play("GameplaySoundtrack");

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

        private bool hydraRanOnce = false;
        public bool demoSceneReadyExit = false;

        void CheckHazardProgress() {
            if (currentHazardManager == null || currentHazardManager.hasEnded) {
                if (currentHazardManager != null) {
                    Destroy(currentHazardManager.gameObject);
                    currentHazardManager = null;
                }
                //new hazard or enemy.
                                //TODO replace this, for now it is the demo hydra
                if (!hydraRanOnce) {
                    hydraRanOnce = true;
                    GameObject hydra = Instantiate(hydraPrefab, new Vector3(70, 5, 0), Quaternion.identity);
                    StartCoroutine(MoveHydra(hydra));
                }
            }
        }

        IEnumerator MoveHydra(GameObject hydra) {
            //float movePerSecond = 5f;
            //while (hydra.transform.position.x > 47) {
                //hydra.transform.position = hydra.transform.position + (Vector3.left * Time.deltaTime * movePerSecond);
                //yield return new WaitForEndOfFrame();
            //}
            yield return new WaitForSeconds(15f);

            Animator hydraAnimator = hydra.GetComponent<Animator>();
            hydraAnimator.SetTrigger("Appear");
            while (!demoSceneReadyExit) {
                yield return null;
            }
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("ToBeContinued");
        }

        // check if any sails remain unbroken
        // trigger gameover if none remain
        public void Update() {
            if (environmentTesting) return;
            int num_surviving_sails = 0;
            foreach (GameObject sail in shipSails) {
                 if (sail.GetComponent<Ship.Sails>().isBroken == false) {
                    num_surviving_sails++;
                }
            }
            if (num_surviving_sails == 0) {
                TriggerGameOver();
            } else if (num_surviving_sails == 1) {
                if (!sound.IsPlaying("ShipLowHP")) {
                    sound.Play("ShipLowHP");
                }
                helpPrompts.DisplayPromptWithDeactivationCondition(helpPrompts.criticalShipPrompt, () => {
                     int num_surviving_sails = 0;
                    foreach (GameObject sail in shipSails) {
                        if (sail.GetComponent<Ship.Sails>().isBroken == false) {
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

            CheckHazardProgress();
        }
    }
}