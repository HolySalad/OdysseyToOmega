using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.HazardManagers.MeteorShowerSubclasses;

namespace SpaceBoat.HazardManagers {
    public class MeteorShower : MonoBehaviour, IHazardManager
    {
        [Header("General Hazard Settings")]
        [SerializeField] private bool testMode = false; 
        [SerializeField] private float baseDuration = 120f; //how many seconds into the game does the last meteor spawn.

        [Header("Meteor Settings")]
        [SerializeField] private GameObject meteorPrefab;

        [Header("Meteor Spawn Rate")]
        [SerializeField] private float firstMeteorSpawnTimer = 10f; //how many seconds into the game does the first meteor spawn.
        [SerializeField] private float meteorInterval = 32f; // how often does a meteor spawn to break a sail by default.
        [SerializeField] private float meteorIntervalVariance = 0.2f; //% variance in meteor base interval.
        [SerializeField] private float meteorIntervalRampTime = 40f; //begins decreasing the time between spawns this many seconds after the first meteor spawns.
        [SerializeField] private float meteorIntervalRampRate = 3f; //how much to decrease the time between spawns by.
        [SerializeField] private float meteorIntervalNumRamps = 5f; //how many times to decrease the time between spawns.
        [SerializeField] private int meteorEaseOffIntervalSails = 3; // begin increasing the interval between meteors after this many sails are already broken.
        [SerializeField] private float meteorEaseOfIntervalRamp = 10f; // increases the interval by this many seconds per sail.
        [SerializeField] private float fullyRepairedTimerReduction = 0.7f; // reduces the interval by this proportion if all sails are repaired.

        [Header("Meteor Projectile Settings")]
        [SerializeField] private float meteorSpawnXPos = 56f; //where do meteors spawn on the X axis
        [SerializeField] private float meteorSpawnYPos = 31f; //where do meteors spawn on the Y axis
        [SerializeField] private float meteorSpawnXVariance = 15f; //how much variance is there in the X spawn position of meteors
        [SerializeField] private float meteorSpawnYVariance = 5f; //how much variance is there in the Y spawn position of meteors
        [SerializeField] private float meteorSpeed = 15f; //how many units per second do meteors travel.

        [Header("Space Rock Settings")]
        [SerializeField] private GameObject rockPrefab;
        [SerializeField] private List<GameObject> rockEmiters;

        [Header("Space Rock Spawn Rate")]
        [SerializeField] private float firstRockSpawnTimer = 30f; //how many seconds into the game does the first rock spawn.

        [SerializeField] private float rockVolleyBaseInterval = 8f; // how long does a volley of rocks last.
        [SerializeField] private float rockVolleyIntervalVariance = 0.2f; //% variance in rock volley base interval.
        [SerializeField] private float peakRockPaceSwell = 2f; // how many more rocks should be thrown during a swell.
        [SerializeField] private float firstSwellTimer = 70f; // how many seconds into the game does the first swell begin
        [SerializeField] private float swellCycleTime = 60f; // how often does the pace of rocks swell.
        [SerializeField] private float swellCycleRampTime = 10f; // Time between swells get shorter each tiime by this many seconds
        [SerializeField] private float swellCycleMinTime = 30f; // how short can the time between swells get.
        [SerializeField] private float baseRockRate = 3f; // how many base rocks are thrown at the player at once.
        [SerializeField] private float firstExtraRockAddedTime = 150f; // how many seconds into the game does an additional base rock get added
        [SerializeField] private float secondExtraRockAddedTime = 210f; // how often does an additional base rock get added
        [Header("Rock Projectile Settings")]
        [SerializeField] private float rockSpeed = 1f; //how many units per second do rocks travel
        [SerializeField] private float rockSpeedVariance = 0.25f; //how much variance is there in the speed of rocks
        [SerializeField] private float rockSpawnHeightVariance = 2f; //units of variance up or down for rock spawn positions
        [SerializeField] private float rockAngleOffset = 5f; // upwards tilt for spawned rocks.
        [SerializeField] private float rockAngleVariance = 5f; //degrees of variance in the angle of rocks from the default 270 degrees
        [SerializeField] private float rockBaseSize = 1f; //base size of rocks
        [SerializeField] private float rockSizeIncreaseVariance = 1.5f; //how much bigger can rocks be?
        [SerializeField] private int rockSoundChance = 50; //% chance that a rock will play a sound when it spawns.
        public bool hasEnded {get; private set;} = false;
        public float hazardDuration {get; private set;} = 0f;
        public float hazardBeganTime {get; private set;} = -1;

        private float nextMeteorSpawnTime; //when is the next meteor?
        private bool isNextMeteorSpawnTimeSet = false; //has the next meteor spawn time been set yet?
        private bool firstMeteor = false; //is this the first meteor?

        private float meteorSoundDuration;
        private int lastSailIndex = -1; //the index of the last sail that was broken by a meteor.

        private bool rocksStarted = false; //have rocks started spawning yet?
        private float rockVolleyStartTime; //when did the last volley begin?
        private float rockSwellStartTime; //when did the last swell begin?
        private float nextSwellTime; //when is the next swell?
        private bool rockSwellActive = false; //is the swell ramping up or down?
        private float currentSwellStrength = 1f; // between 1 and 1+peakRockPaceSwell

        void handleMeteorSpawning(float timeSinceGameBegan, float deltaTime)
        {
            List<GameObject> sails = GameModel.Instance.shipSails;
            List<GameObject> nonbrokenSails = new List<GameObject>();
            foreach (GameObject sail in sails)
            {
                if (!sail.GetComponent<Ship.SailsActivatable>().isBroken)
                {
                    nonbrokenSails.Add(sail);
                }
            }
            int targetSailIndex = Random.Range(0, nonbrokenSails.Count);
            if (nonbrokenSails.Count > 1 && targetSailIndex == lastSailIndex)
            {
                targetSailIndex = (targetSailIndex + 1) % nonbrokenSails.Count;
            }
            GameObject randomSail = nonbrokenSails[targetSailIndex];
            lastSailIndex = targetSailIndex;
            float xPos = meteorSpawnXPos + Random.Range(-meteorSpawnXVariance, meteorSpawnXVariance);
            float yPos = meteorSpawnYPos + Random.Range(-meteorSpawnYVariance, meteorSpawnYVariance);
            GameObject meteorObject = Instantiate(meteorPrefab, new Vector2(xPos, yPos), Quaternion.identity);
            Meteorite meteor = meteorObject.GetComponent<Meteorite>();
            meteor.SetupMeteor(meteorSpeed, meteorObject.transform.position, randomSail, meteorSoundDuration);   
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
            List<GameObject> sails = GameModel.Instance.shipSails;
            List<GameObject> nonbrokenSails = new List<GameObject>();
            foreach (GameObject sail in sails)
            {
                if (!sail.GetComponent<Ship.SailsActivatable>().isBroken)
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

        IEnumerator RockSpawner(GameObject emiter, float timeSinceGameBegan, float deltaTime) {
            float nextRockSpawn = timeSinceGameBegan + Random.Range(0, rockVolleyBaseInterval);
            while (!hasEnded) {
                if (Time.time >= nextRockSpawn) {
                    float height = emiter.transform.position.y + Random.Range(-rockSpawnHeightVariance, rockSpawnHeightVariance);
                    GameObject rockObject = Instantiate(rockPrefab, new Vector2(emiter.transform.position.x, height), Quaternion.identity);
                    SpaceRock rock = rockObject.GetComponent<SpaceRock>();
                    float speed = rockSpeed* (1- Random.Range(-rockSpeedVariance, rockSpeedVariance));
                    float angle = rockAngleOffset + Random.Range(-rockAngleVariance, rockAngleVariance);
                    float scale = rockBaseSize * (1 + Random.Range(0, rockSizeIncreaseVariance));
                    bool sound = Random.Range(0, 100) < rockSoundChance;
                    rock.SetupRock(speed, angle, scale, sound);
                    nextRockSpawn = Time.time + rockVolleyBaseInterval / (GetCurrentRockRate(hazardBeganTime - Time.time) * (1 - Random.Range(-rockVolleyIntervalVariance, rockVolleyIntervalVariance)));
                }
                yield return null;
            }
        }

        float GetCurrentRockRate(float timeSinceGameBegan) {
            float currentRockRate = baseRockRate;
            if (rockSwellActive) {
                currentRockRate = Mathf.Floor(currentSwellStrength*currentRockRate);
            }
            if (timeSinceGameBegan > firstExtraRockAddedTime)
            {
                currentRockRate += 1;
            }

            if (timeSinceGameBegan > secondExtraRockAddedTime)
            {
                currentRockRate += 1;
            }
            return currentRockRate;
        }
        void FixedUpdate() {
            if (hazardBeganTime < 0) {
                return;
            } else if (hasEnded) {
                return;
            }
            float deltaTime = Time.fixedDeltaTime;
            float timeSinceStart = Time.time - hazardBeganTime;

            if (timeSinceStart > hazardDuration) {
                Debug.Log("Hazard " + this.gameObject.name + " has ended");
                hazardBeganTime = -1;
                hasEnded = true;
                return;
            }

            if (timeSinceStart > firstMeteorSpawnTimer && timeSinceStart < hazardDuration) {
                if (isNextMeteorSpawnTimeSet) {
                    if (nextMeteorSpawnTime < timeSinceStart) {
                        handleMeteorSpawning(timeSinceStart, deltaTime);
                    }
                } else if (!isNextMeteorSpawnTimeSet) {
                    calcNextMeteorSpawnTime(timeSinceStart, deltaTime);
                }
            }

            // handle rock swells.
            if (rockSwellActive) {
                float swellTime = timeSinceStart - rockSwellStartTime;
                if (swellTime < swellCycleTime) {
                    currentSwellStrength = (peakRockPaceSwell * (swellTime / swellCycleTime));
                } else if (swellTime < 2*swellCycleTime) {
                    currentSwellStrength = (peakRockPaceSwell * (1 - ((swellTime - swellCycleTime) / swellCycleTime)));
                } else {
                    rockSwellActive = false;
                    swellCycleTime = Mathf.Max(swellCycleTime - swellCycleRampTime , swellCycleMinTime );
                    nextSwellTime = timeSinceStart + (swellCycleTime/2);
                }
            } else if (nextSwellTime < timeSinceStart) {
                rockSwellActive = true;
                rockSwellStartTime = timeSinceStart;
                Debug.Log("Starting rock swell at " + timeSinceStart);
            }

            if (timeSinceStart > firstRockSpawnTimer && !rocksStarted) {
                Debug.Log("Starting Rock Volley. Time since game began: " + timeSinceStart + " Rock volley start time: " + rockVolleyStartTime + " Rock volley length: " + rockVolleyBaseInterval);
                foreach (GameObject emiter in rockEmiters) {
                    StartCoroutine(RockSpawner(emiter, timeSinceStart, deltaTime));
                }
                rocksStarted = true;
            } 
        }

        public void StartHazard() {
            Debug.Log("Starting hazard " + this.gameObject.name);
            hazardDuration = baseDuration;
            hazardBeganTime = Time.time;
            meteorSoundDuration = SoundManager.Instance.Length("MeteorWhoosh_0");
            nextSwellTime = hazardBeganTime + firstSwellTimer;
        }

        void Start() {
            if (testMode) {
                StartHazard();
            }
        }
    }
}