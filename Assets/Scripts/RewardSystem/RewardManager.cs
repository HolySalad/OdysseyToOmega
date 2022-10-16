using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;

namespace SpaceBoat.RewardSystem {
    public class RewardManager : MonoBehaviour
    {
        [Header("System Setup Ask Chris if Broken")]
        [SerializeField] private GameObject clothPrefab;
        [SerializeField] private GameObject harpoonPrefab;
        [SerializeField] private GameObject foodPrefab;
        [SerializeField] private GameObject cometPrefab;
        [SerializeField] private GameObject targetObject;
        [SerializeField] private GameObject pityTarget;
        [SerializeField] private GameObject playerCharacter;


        [Header("Reward Occurance")]
        [SerializeField] private int clothRewardRelativeWeight = 5;
        [SerializeField] private int harpoonRewardRelativeWeight = 1;
        [SerializeField] private int foodRewardRelativeWeight = 2;

        [Header("Reward Comet Projectile Settings")]
        [SerializeField] private float projectileSpeed = 8f;
        [SerializeField] private float cometSpawnXPos = 10f; //where do comets spawn on the X axis
        [SerializeField] private float cometSpawnYPos = 5f; //where do comets spawn on the Y axis
        [SerializeField] private float cometSpawnXVariance = 5f; //how much variance is there in the X spawn position of comets
        [SerializeField] private float cometSpawnYVariance = 5f; //how much variance is there in the Y spawn position of comets
        [SerializeField] private int cometPityChance = 100;
        [SerializeField] private float cometAngleOffset = 5f; // upwards tilt for spawned comets.
        [SerializeField] private float cometAngleVariance = 5f; //degrees of variance in the angle of comets from the default 270 degrees
        

        [Header("Reward Comet Spawn Settings")]
        [SerializeField] private float firstCometSpawnTimer = 50f; //how many seconds into the game does the first comet spawn.
        [SerializeField] private float lastCometSpawnTimer = 280f; //how many seconds into the game does the last comet spawn.

        [SerializeField] private float cometInterval = 32f; // how often does a comet spawn to break a sail by default.
        [SerializeField] private float cometIntervalVariance = 0.2f; //% variance in comet base interval.
        [SerializeField] private float cometIntervalRampTime = 40f; //begins decreasing the time between spawns this many seconds after the first comet spawns.
        [SerializeField] private float cometIntervalRampRate = 3f; //how much to decrease the time between spawns by.
        [SerializeField] private float cometIntervalNumRamps = 5f; //how many times to decrease the time between spawns.
        

        private float nextCometSpawnTime; //when is the next comet?
        private bool isNextCometSpawnTimeSet = false; //has the next comet spawn time been set yet?
        private bool firstComet = false; //is this the first comet?

        private float gameBeganTime;

        void Awake() {
            gameBeganTime = Time.time;
        }

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

        void handleCometSpawning(float timeSinceGameBegan, float deltaTime)
        {
            int rand_range = clothRewardRelativeWeight + harpoonRewardRelativeWeight + foodRewardRelativeWeight;
            int rand = Random.Range(0, rand_range);
            string itemType = "";
            if (rand <= clothRewardRelativeWeight) {
                itemType = "ClothItem";
            } else if (rand <= clothRewardRelativeWeight + harpoonRewardRelativeWeight) {
                itemType = "HarpoonItem";
            } else {
                itemType = "FoodItem";
            }
            float xPos = cometSpawnXPos + Random.Range(-cometSpawnXVariance, cometSpawnXVariance);
            float yPos = cometSpawnYPos + Random.Range(-cometSpawnYVariance, cometSpawnYVariance);
            float angle = cometAngleOffset + Random.Range(-cometAngleVariance, cometAngleVariance);
            GameObject cometObject = Instantiate(cometPrefab, new Vector2(xPos, yPos), Quaternion.identity);
            Comet comet = cometObject.GetComponent<Comet>();
            GameObject target = targetObject;
            if (Random.Range(0, 100) <= cometPityChance) {
                target = pityTarget;
            }
            comet.SetupComet(projectileSpeed, cometObject.transform.position, target, PrefabForItemType(itemType), itemType, playerCharacter);   
            isNextCometSpawnTimeSet = false;
        }

        void calcNextCometSpawnTime(float timeSinceGameBegan, float deltaTime) {
            Debug.Log("Determining next comet spawn at " + timeSinceGameBegan);
            if (!firstComet) {
                handleCometSpawning(timeSinceGameBegan, deltaTime);
                isNextCometSpawnTimeSet = true;
                firstComet = true;
                 Debug.Log("Next comet will spawn at " + nextCometSpawnTime);
            }
            float nextInterval = cometInterval;
            nextInterval *= (1 - Random.Range(-cometIntervalVariance, cometIntervalVariance));
            float intervalRamp = Mathf.Min(((timeSinceGameBegan - firstCometSpawnTimer) / cometIntervalRampTime), cometIntervalNumRamps) * cometIntervalRampRate;
            nextInterval -= intervalRamp;
            nextCometSpawnTime = timeSinceGameBegan + nextInterval;
            Debug.Log("Next comet will spawn at " + nextCometSpawnTime);
            isNextCometSpawnTimeSet = true;
        }


        void FixedUpdate() {
            float deltaTime = Time.fixedDeltaTime;
            float timeSinceGameBegan = Time.time - gameBeganTime;

            if (timeSinceGameBegan > firstCometSpawnTimer && timeSinceGameBegan < lastCometSpawnTimer) {
                if (isNextCometSpawnTimeSet && nextCometSpawnTime < timeSinceGameBegan) {
                    handleCometSpawning(timeSinceGameBegan, deltaTime);
                } else if (!isNextCometSpawnTimeSet) {
                    calcNextCometSpawnTime(timeSinceGameBegan, deltaTime);
                }
            }
        }
        
    }
}