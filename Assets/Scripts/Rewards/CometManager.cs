using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
namespace SpaceBoat.Rewards {    
    public class CometManager : MonoBehaviour
    {
        [SerializeField] private GameObject cometPrefab;
        [SerializeField] private float cometSpawnRate = 15f; // seconds between spawns
        [SerializeField] private float cometSpawnRateVariance = 4f; // seconds between spawns
        [SerializeField] private float cometPityMaxRate = 45f; // seconds between spawns

        [SerializeField] private float cometSpawnX = 47f;
        [SerializeField] private float cometSpawnY = 20f;
        [SerializeField] private float cometSpawnYVariance = 2f;

        [SerializeField] private int cometFoodChance = 1;
        [SerializeField] private int cometFoodNumber = 1;
        [SerializeField] private int cometClothChance = 3;
        [SerializeField] private int cometClothNumber = 1;
        
        [SerializeField] private Dictionary<ItemTypes, int> itemQuantities = new Dictionary<ItemTypes, int>{

        };
        
        [SerializeField] private float firstCometTime = 20f;

        private float cometSpawnTimer = -5f;
        private float cometPityTimer = 0f;
        private List<ItemTypes> randomItemSelector = new List<ItemTypes>();
        private bool isFirstComet = true;
        public void Awake() {
            for (int i = 0; i < cometFoodChance; i++) {
                randomItemSelector.Add(ItemTypes.FoodItem);
            }
            for (int i = 0; i < cometClothChance; i++) {
                randomItemSelector.Add(ItemTypes.ClothItem);
            }
            itemQuantities.Add(ItemTypes.FoodItem, cometFoodNumber);
            itemQuantities.Add(ItemTypes.ClothItem, cometClothNumber);
        }

        void SpawnComet(bool isPity, ItemTypes itemType) {
            float x = cometSpawnX;
            float y = Random.Range(cometSpawnY - cometSpawnYVariance, cometSpawnY + cometSpawnYVariance);
            Vector3 spawnPosition = new Vector3(x, y, 0);
            GameObject comet = Instantiate(cometPrefab, spawnPosition, Quaternion.identity);
            RewardComet rewardComet = comet.GetComponent<RewardComet>();
            rewardComet.SetItemType(itemType);
            rewardComet.LaunchComet(isPity);
            rewardComet.numItems = itemQuantities[itemType];
            if (isFirstComet) {
                isFirstComet = false;
                Debug.Log("First comet: displaying help text");
                //GameModel.Instance.helpPrompts.DisplayPrompt(GameModel.Instance.helpPrompts.cometPrompt);
            }
        }

        (bool, ItemTypes) CheckNeededItemsAndPity() {
            ClothItem[] clothItems = FindObjectsOfType<ClothItem>();
            FoodItem[] foodItems = FindObjectsOfType<FoodItem>();
            Debug.Log("Checking item totals for comet spawn:");
            Debug.Log("Cloth items: " + clothItems.Length);
            Debug.Log("Food items: " + foodItems.Length);
            if (clothItems.Length < 1) {
                return (true, ItemTypes.ClothItem);
            }

            if (foodItems.Length < 1 && GameModel.Instance.player.health < GameModel.Instance.player.maxHealth) {
                return (true, ItemTypes.FoodItem);
            }
            ItemTypes itemType = randomItemSelector[Random.Range(0, randomItemSelector.Count)];
            return (false, itemType);
        }

        public void Update() {
            if (cometSpawnTimer < 0f) {
                cometSpawnTimer = firstCometTime;
                return;
            }
            if (Time.time > cometSpawnTimer ) {
                (bool shouldPity, ItemTypes itemType) = CheckNeededItemsAndPity();
                if (shouldPity && Time.time > cometPityTimer) {
                    SpawnComet(true, itemType);
                    cometPityTimer = Time.time + cometPityMaxRate;
                    cometSpawnTimer = Time.time + cometSpawnRate + Random.Range(-cometSpawnRateVariance, cometSpawnRateVariance);
                    Debug.Log("Pity comet spawned, next comet in " + (cometPityTimer - Time.time) + " seconds");
                } else {
                    SpawnComet(false, itemType);
                    cometSpawnTimer = Time.time + cometSpawnRate + Random.Range(-cometSpawnRateVariance, cometSpawnRateVariance);
                    Debug.Log("Comet spawned, next comet in " + (cometSpawnTimer - Time.time) + " seconds");
                }
            }
        }
    }
}
*/