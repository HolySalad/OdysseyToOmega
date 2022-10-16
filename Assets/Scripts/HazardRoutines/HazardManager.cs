using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Hazards {
    public class HazardManager : MonoBehaviour
    {
        [Header("Hazard Game Objects")]
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private GameObject meteorPrefab;
        [SerializeField] private GameObject asteroidPrefab;

        [Header("Hazard Spawn Rate")]
        [SerializeField] private float firstRockSpawnTimer = 30f; //how many seconds into the game does the first rock spawn.
        [SerializeField] private float firstMeteorSpawnTimer = 50f; //how many seconds into the game does the first meteor spawn.
        [SerializeField] private float lastRockSpawnTimer = 290f; //how many seconds into the game does the last rock spawn.
        [SerializeField] private float lastMeteorSpawnTimer = 280f; //how many seconds into the game does the last meteor spawn.

        [SerializeField] private float meteorInterval = 32f; // how often does a meteor spawn to break a sail by default.
        [SerializeField] private float meteorIntervalVariance = 0.2f; //% variance in meteor base interval.
        [SerializeField] private float meteorIntervalRampTime = 40f; //begins decreasing the time between spawns this many seconds after the first meteor spawns.
        [SerializeField] private float meteorIntervalRampRate = 3f; //how much to decrease the time between spawns by.
        [SerializeField] private float meteorIntervalNumRamps = 5f; //how many times to decrease the time between spawns.
        [SerializeField] private int meteorEaseOffIntervalSails = 3; // begin increasing the interval between meteors after this many sails are already broken.
        [SerializeField] private float meteorEaseOfIntervalRamp = 10f; // increases the interval by this many seconds per sail.
        [SerializeField] private float fullyRepairedTimerReduction = 0.7f; // reduces the interval by this proportion if all sails are repaired.

        // the pace of rocks flying at the player swells and subsides over time

        [SerializeField] private float rockVolleyLength = 8f; // how long does a volley of rocks last.
        [SerializeField] private float peakRockPaceSwell = 2f; // how many more rocks should be thrown during a swell.
        [SerializeField] private float firstSwellTimer = 70f; // how many seconds into the game does the first swell begin
        [SerializeField] private float swellCycleTime = 60f; // how often does the pace of rocks swell.
        [SerializeField] private float swellCycleRampTime = 40f; // Time between swells get shorter each tiime by this many seconds
        [SerializeField] private float swellCycleMinTime = 30f; // how short can the time between swells get.
        
        [SerializeField] private float numRocksPerVolley = 3f; // how many base rocks are thrown at the player at once.
        [SerializeField] private float firstExtraRockAddedTime = 150f; // how many seconds into the game does an additional base rock get added
        [SerializeField] private float secondExtraRockAddedTime = 210f; // how often does an additional base rock get added





        [Header("Boss Fight/End Game")]
        [SerializeField] private float bossFightStartTimer = 300f; // how many seconds into the game does the boss fight start
        [Header("Rock Projectile Settings")]
        [SerializeField] private float rockSpeed = 1f; //how many units per second do rocks travel
        [SerializeField] private float rockSpeedVariance = 0.25f; //how much variance is there in the speed of rocks
        [SerializeField] private float baseRockSpawnHeight = 5f; //what Y pos do rocks spawn at
        [SerializeField] private float rockSpawnHeightVariance = 2f; //units of variance up or down for rock spawn positions
        [SerializeField] private float rockAngleOffset = 5f; // upwards tilt for spawned rocks.
        [SerializeField] private float rockAngleVariance = 5f; //degrees of variance in the angle of rocks from the default 270 degrees
        [SerializeField] private float rockBaseSize = 1f; //base size of rocks
        [SerializeField] private float rockSizeIncreaseVariance = 1.5f; //how much bigger can rocks be?
        [Header("Meteor Projectile Settings")]
        [SerializeField] private float meteorSpawnXPos = 10f; //where do meteors spawn on the X axis
        [SerializeField] private float meteorSpawnYPos = 5f; //where do meteors spawn on the Y axis
        [SerializeField] private float meteorSpawnXVariance = 5f; //how much variance is there in the X spawn position of meteors
        [SerializeField] private float meteorSpawnYVariance = 5f; //how much variance is there in the Y spawn position of meteors
        [SerializeField] private float meteorSpeed = 25f; //how many units per second do meteors travel

        private bool hasMusicPlayed = false;


        private float gameBeganTime;

        private float rockVolleyStartTime; //when did the last volley begin?
        private float rockSwellStartTime; //when did the last swell begin?
        
        private float nextSwellTime; //when is the next swell?
        private bool rockSwellActive = false; //is the swell ramping up or down?
        private float currentSwellStrength = 1f; // between 1 and 1+peakRockPaceSwell
        private float horizontalSpawnCoordinate = 65f; //where do rocks spawn horizontally?

        private float nextMeteorSpawnTime; //when is the next meteor?
        private bool isNextMeteorSpawnTimeSet = false; //has the next meteor spawn time been set yet?
        private bool firstMeteor = false; //is this the first meteor?

        void Awake() {
            gameBeganTime = Time.time;
        }

        GameObject[] FindGameObjectsInLayer(int layer){
            var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
            var goList = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == layer)
                {
                    goList.Add(goArray[i]);
                }
            }
            if (goList.Count == 0)
            {
                return null;
            }
            return goList.ToArray();
        }

        void handleRockSpawning(float timeSinceGameBegan, float deltaTime) {
            float numRocks = numRocksPerVolley;
            if (rockSwellActive) {
                numRocks = Mathf.Floor(currentSwellStrength*numRocks);
            }
            if (timeSinceGameBegan > firstExtraRockAddedTime)
            {
                numRocks += 1;
            }

            if (timeSinceGameBegan > secondExtraRockAddedTime)
            {
                numRocks += 1;
            }
            rockVolleyStartTime = timeSinceGameBegan;
            float baseRockInterval = rockVolleyLength / numRocks;
            for (int i = 0; i < numRocks; i++) {
                GameObject rockObject = Instantiate(rockPrefab, new Vector2(horizontalSpawnCoordinate, baseRockSpawnHeight), Quaternion.identity);
                SpaceRock rock = rockObject.GetComponent<SpaceRock>();
                float speed = rockSpeed* (1- Random.Range(-rockSpeedVariance, rockSpeedVariance));
                float angle = rockAngleOffset + Random.Range(-rockAngleVariance, rockAngleVariance);
                float scale = rockBaseSize * (1 + Random.Range(0, rockSizeIncreaseVariance));
                float height = baseRockSpawnHeight + Random.Range(-rockSpawnHeightVariance, rockSpawnHeightVariance);
                float launchTime = Mathf.Min(rockVolleyStartTime + (Random.Range(0.75f, 1.25f) * baseRockInterval * i), rockVolleyStartTime + rockVolleyLength);
                rock.SetupRock(speed, angle, scale, height, launchTime);
            }
            
        }
    
        void handleMeteorSpawning(float timeSinceGameBegan, float deltaTime)
        {
            List<GameObject> sails = new List<GameObject>(FindGameObjectsInLayer(LayerMask.NameToLayer("Sails")));
            List<GameObject> nonbrokenSails = new List<GameObject>();
            foreach (GameObject sail in sails)
            {
                if (!sail.GetComponent<Sails>().IsBroken)
                {
                    nonbrokenSails.Add(sail);
                }
            }
            GameObject randomSail = nonbrokenSails[Random.Range(0, nonbrokenSails.Count)];
            float xPos = meteorSpawnXPos + Random.Range(-meteorSpawnXVariance, meteorSpawnXVariance);
            float yPos = meteorSpawnYPos + Random.Range(-meteorSpawnYVariance, meteorSpawnYVariance);
            GameObject meteorObject = Instantiate(meteorPrefab, new Vector2(xPos, yPos), Quaternion.identity);
            Meteorite meteor = meteorObject.GetComponent<Meteorite>();
            meteor.SetupMeteor(meteorSpeed, meteorObject.transform.position, randomSail);   
            isNextMeteorSpawnTimeSet = false;
        }

        void calcNextMeteorSpawnTime(float timeSinceGameBegan, float deltaTime) {
            Debug.Log("Determining next meteor spawn at " + timeSinceGameBegan);
            if (!firstMeteor) {
                handleMeteorSpawning(timeSinceGameBegan, deltaTime);
                isNextMeteorSpawnTimeSet = true;
                firstMeteor = true;
                 Debug.Log("Next meteoer will spawn at " + nextMeteorSpawnTime);
            }
            List<GameObject> sails = new List<GameObject>(FindGameObjectsInLayer(LayerMask.NameToLayer("Sails")));
            List<GameObject> nonbrokenSails = new List<GameObject>();
            foreach (GameObject sail in sails)
            {
                if (!sail.GetComponent<Sails>().IsBroken)
                {
                    nonbrokenSails.Add(sail);
                }
            }
            float nextInterval = meteorInterval;
            nextInterval *= (1 - Random.Range(-meteorIntervalVariance, meteorIntervalVariance));
            float intervalRamp = Mathf.Min(((timeSinceGameBegan - firstMeteorSpawnTimer) / meteorIntervalRampTime), meteorIntervalNumRamps) * meteorIntervalRampRate;
            nextInterval -= intervalRamp;
            int brokenSailCount = sails.Count - nonbrokenSails.Count;
            if (brokenSailCount >= meteorEaseOffIntervalSails) {
                nextInterval += meteorEaseOfIntervalRamp*(brokenSailCount - meteorEaseOffIntervalSails + 1);
            } else if (brokenSailCount == 0) {
                nextInterval *= fullyRepairedTimerReduction;
            }
            nextMeteorSpawnTime = timeSinceGameBegan + nextInterval;
            Debug.Log("Next meteoer will spawn at " + nextMeteorSpawnTime);
            isNextMeteorSpawnTimeSet = true;
        }


        void FixedUpdate() {
            float deltaTime = Time.fixedDeltaTime;
            float timeSinceGameBegan = Time.time - gameBeganTime;

            // handle rock swells.
            if (rockSwellActive) {

            } else if (nextSwellTime < timeSinceGameBegan) {
                
            } else {
                nextSwellTime = timeSinceGameBegan + firstSwellTimer;
            }
            if (timeSinceGameBegan > firstRockSpawnTimer && timeSinceGameBegan < lastRockSpawnTimer && (rockVolleyStartTime + rockVolleyLength < timeSinceGameBegan)) {
                Debug.Log("Starting Rock Volley. Time since game began: " + timeSinceGameBegan + " Rock volley start time: " + rockVolleyStartTime + " Rock volley length: " + rockVolleyLength);
                handleRockSpawning (timeSinceGameBegan, deltaTime);
            } 
            if (timeSinceGameBegan > firstMeteorSpawnTimer && timeSinceGameBegan < lastMeteorSpawnTimer) {
                if (isNextMeteorSpawnTimeSet && nextMeteorSpawnTime < timeSinceGameBegan) {
                    handleMeteorSpawning(timeSinceGameBegan, deltaTime);
                } else if (!isNextMeteorSpawnTimeSet) {
                    calcNextMeteorSpawnTime(timeSinceGameBegan, deltaTime);
                }
            }
            SoundManager sm = FindObjectOfType<SoundManager>();
            // soundtrack
            if (!sm.IsPlaying("GameplaySoundtrack")) {
                sm.Play("GameplaySoundtrack");
            }

        }

    }
}
