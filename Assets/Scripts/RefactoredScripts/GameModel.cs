using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;


namespace SpaceBoat {
    public class GameModel : MonoBehaviour
    {

        [SerializeField] private GameObject[] shipSails;
        
        // item prefabs
        [SerializeField] public GameObject clothPrefab;
        [SerializeField] public GameObject harpoonPrefab;
        [SerializeField] public GameObject foodPrefab;

        public Player player {get; private set;}
        public SoundManager sound {get; private set;}


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

        void Awake() {
            // This is a singleton, so if there is already a GameModel in the scene, destroy this one.
            if (FindObjectsOfType<GameModel>().Length > 1) {
                Destroy(gameObject);
            }

            // Find the playerCharacter 
            player = FindObjectOfType<Player>();
            sound = FindObjectOfType<SoundManager>();
        }

        void Start() {
            sound.Play("GameplaySoundtrack");
        }
    }
}