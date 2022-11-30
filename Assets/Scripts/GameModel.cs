using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;
using SpaceBoat.HazardManagers;
using UnityEngine.SceneManagement;


namespace SpaceBoat {
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

        public float GameBeganTime {get; private set;}


        //item management
        public string GetItemType(GameObject item) {
            if (item.GetComponent<ClothItem>() != null) {
                return "ClothItem";
            } else if (item.GetComponent<HarpoonItem>() != null) {
                return "HarpoonItem";
            } else if (item.GetComponent<FoodItem>() != null) {
                return "FoodItem";
            }
            return "";
        }

        public IHeldItems CreateItemComponent(GameObject target, string itemType) {
            if (itemType == "ClothItem") {
                return target.AddComponent<ClothItem>();
            } else if (itemType == "HarpoonItem") {
                return target.AddComponent<HarpoonItem>();
            } else if (itemType == "FoodItem") {
                return target.AddComponent<FoodItem>();
            }
            return null;
        }


        public GameObject PrefabForItemType(string itemType) {
            if (itemType == "ClothItem") {
                return clothPrefab;
            } else if (itemType == "HarpoonItem") {
                return harpoonPrefab;
            } else if (itemType == "FoodItem") {
                return foodPrefab;
            }
            return null;
        }

        //hazard management
        public IHazardManager CreateHazardManager(string hazardManagerType) {
            if (hazardManagerType == "MeteorShower") {
                return Instantiate(meteorManagerPrefab).GetComponent<MeteorShower>();
            }
            return null;
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

            GameBeganTime = Time.time;
        }

        public void Start() {
            Debug.Log("Game is starting!");
            sound.Play("Spawn");
            sound.Play("GameplaySoundtrack");

            //TODO add random hazard selection.
            CreateHazardManager("MeteorShower").StartHazard();

        }

        public IEnumerator GameOver() {
            Debug.Log("Gameover!");
            yield return new WaitForSeconds(2);
            SceneManager.LoadScene("GameOver");
        }

        // check if any sails remain unbroken
        // trigger gameover if none remain
        public void OnSailBroken() {
            foreach (GameObject sail in shipSails) {
                if (sail.GetComponent<Ship.Sails>().isBroken == false) {
                    return;
                }
            }
            StartCoroutine(GameOver());
        }

    }
}