using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.HazardManagers {
    public class MeteorShower : MonoBehaviour, IHazardManager
    {
        [SerializeField] private GameObject meteorPrefab;

        [Header("Hazard Spawn Rate")]
        [SerializeField] private float firstMeteorSpawnTimer = 10f; //how many seconds into the game does the first meteor spawn.
        [SerializeField] private float lastMeteorSpawnTimer = 280f; //how many seconds into the game does the last meteor spawn.

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

        private float nextMeteorSpawnTime; //when is the next meteor?
        private bool isNextMeteorSpawnTimeSet = false; //has the next meteor spawn time been set yet?
        private bool firstMeteor = false; //is this the first meteor?
        
        public float hazardBeganTime {get; private set;} = -1;

        private float meteorSoundDuration;

        private int lastSailIndex = -1; //the index of the last sail that was broken by a meteor.

        void handleMeteorSpawning(float timeSinceGameBegan, float deltaTime)
        {
            List<GameObject> sails = GameModel.Instance.shipSails;
            List<GameObject> nonbrokenSails = new List<GameObject>();
            foreach (GameObject sail in sails)
            {
                if (!sail.GetComponent<Ship.Sails>().isBroken)
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
                if (!sail.GetComponent<Ship.Sails>().isBroken)
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
            if (hazardBeganTime < 0) {
                return;
            }
            float deltaTime = Time.fixedDeltaTime;
            float timeSinceStart = Time.time - hazardBeganTime;

            if (timeSinceStart > firstMeteorSpawnTimer && timeSinceStart < lastMeteorSpawnTimer) {
                if (isNextMeteorSpawnTimeSet) {
                    if (nextMeteorSpawnTime < timeSinceStart) {
                        handleMeteorSpawning(timeSinceStart, deltaTime);
                    }
                } else if (!isNextMeteorSpawnTimeSet) {
                    calcNextMeteorSpawnTime(timeSinceStart, deltaTime);
                }
            }
        }

        public void StartHazard() {
            hazardBeganTime = Time.time;
            meteorSoundDuration = SoundManager.Instance.Length("MeteorWhoosh_0");
        }
    }
}