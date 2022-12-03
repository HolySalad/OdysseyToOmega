using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;
using SpaceBoat.HazardManagers;
using SpaceBoat.Ship;
using UnityEngine.SceneManagement;


namespace SpaceBoat {

    public enum ItemTypes {ClothItem, FoodItem, HarpoonItem, None};
    public enum Activatables {HarpoonGun, None};
    public class GameModel : MonoBehaviour
    {
        public static GameModel Instance;

        [Header("Ship")]
        [SerializeField] public List<GameObject> shipSails;
        
        // item prefabs
        [Header("Item Prefabs")]
        [SerializeField] public GameObject clothPrefab;
        [SerializeField] public GameObject harpoonPrefab;
        [SerializeField] public GameObject foodPrefab;

        // hazard manager prefabs
        [Header("Hazard Manager Prefabs")]
        [SerializeField] public GameObject meteorManagerPrefab;


        public Player player {get; private set;}
        public SoundManager sound {get; private set;}

        public UI.HelpPrompts helpPrompts {get; private set;}

        public float GameBeganTime {get; private set;}


    
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
            sound = FindObjectOfType<SoundManager>();
            helpPrompts = FindObjectOfType<UI.HelpPrompts>();

            GameBeganTime = Time.time;
        }

        public void Start() {
            Debug.Log("Game is starting!");
            sound.Play("Spawn");
            if (sound.IsPlaying("MenuSoundtrack")) {
                sound.Stop("MenuSoundtrack");
            }
            sound.Play("GameplaySoundtrack");

            //TODO add random hazard selection.
            CreateHazardManager("MeteorShower").StartHazard();

        }

        public IEnumerator GameOver() {
            Debug.Log("Gameover!");
            yield return new WaitForSeconds(2);
            //SoundManager.Instance.Stop("GameplaySoundtrack");
            SceneManager.LoadScene("GameOver");
        }

        // check if any sails remain unbroken
        // trigger gameover if none remain
        public void OnSailBroken() {
            int num_surviving_sails = 0;
            foreach (GameObject sail in shipSails) {
                if (sail.GetComponent<Ship.Sails>().isBroken == false) {
                    num_surviving_sails++;
                }
            }
            if (num_surviving_sails == 0) {
                StartCoroutine(GameOver());
            } else if (num_surviving_sails == 1) {
                sound.Stop("ShipLowHP");
                sound.Play("ShipLowHP");
                helpPrompts.DisplayPromptWithDeactivationCondition(helpPrompts.criticalShipPrompt, () => {
                     int num_surviving_sails = 0;
                    foreach (GameObject sail in shipSails) {
                        if (sail.GetComponent<Ship.Sails>().isBroken == false) {
                            num_surviving_sails++;
                        }
                    }
                    return (num_surviving_sails < 2);
                });
            }
        }

    }
}