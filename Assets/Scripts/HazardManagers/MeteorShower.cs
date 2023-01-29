using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.HazardManagers.MeteorShowerSubclasses;

namespace SpaceBoat.HazardManagers.MeteorShowerSubclasses {
    [System.Serializable] public class EscalationLevel {
        public float timeIntoHazard = 0f;
        public int baseNumMeteors = 0;
        public float meteorIntervalMultiplier = 1f;
        public float rockRateIncrease = 0f;
        public float rockSpeedIncrease = 0f;
    }
}


namespace SpaceBoat.HazardManagers {
    public class MeteorShower : MonoBehaviour, IHazardManager
    {
        [Header("General Hazard Settings")]        
        [SerializeField] private bool testMode = false; 
        [SerializeField] private float baseDuration = 120f; //how many seconds into the game does the last meteor spawn.
        [SerializeField] private List<UI.HelpPrompt> meteorPrompts;
        [SerializeField] private int earliestAppearence;
        [SerializeField] private int latestAppearence;
        [SerializeField] private int priority;

        [Header("Escalation Settings")]
        [SerializeField] private List<EscalationLevel> escalationLevels;
        [Header("Meteor Settings")]
        [SerializeField] private GameObject meteorPrefab;

        [Header("Meteor Spawn Rate")]
        [SerializeField] private float firstMeteorSpawnTimer = 10f; //how many seconds into the game does the first meteor spawn.
        [SerializeField] private float meteorInterval = 32f; // how often does a meteor spawn to break a sail by default.
        [SerializeField] private float meteorIntervalVariance = 0.2f; //% variance in meteor base interval.
        [SerializeField] private float fullyRepairedTimerReduction = 0.5f; // reduces the interval by this proportion if all sails are repaired.

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
        [SerializeField] private float baseRockRate = 3f; // how many base rocks are thrown at the player at once.
        [Header("Rock Projectile Settings")]
        [SerializeField] private float rockSpeed = 1f; //how many units per second do rocks travel
        [SerializeField] private float rockSpeedVariance = 0.25f; //how much variance is there in the speed of rocks
        [SerializeField] private float rockSpawnHeightVariance = 2f; //units of variance up or down for rock spawn positions
        [SerializeField] private float rockAngleOffset = 5f; // upwards tilt for spawned rocks.
        [SerializeField] private float rockAngleVariance = 5f; //degrees of variance in the angle of rocks from the default 270 degrees
        [SerializeField] private float rockBaseSize = 1f; //base size of rocks
        [SerializeField] private float rockSizeIncreaseVariance = 1.5f; //how much bigger can rocks be?
        [SerializeField] private int rockSoundChance = 50; //% chance that a rock will play a sound when it spawns.

        public string hazardSoundtrack { get; private set; } = "FirstGalaxy";


        public bool hasEnded {get; private set;} = false;
        public float hazardDuration {get; private set;} = 0f;
        public float hazardBeganTime {get; private set;} = -1;

        private float meteorSoundDuration;

        private bool rocksStarted = false; 
        private bool meteorsStarted = false; 
        private bool startupSequence = false;
        private bool fullyRepairedIncreaseApplied = true;
        private int meteorsOut = 0;

        private EscalationLevel currentEscalationLevel;
        private int nextEscalationIndex = 0;

        public int GetPriority() {
            return priority;
        }
        public int GetEarliestAppearence() {
            return earliestAppearence;
        }
        public int GetLatestAppearence() {
            return latestAppearence;
        }

        public void meteorHit() {
            meteorsOut--;
        }

        float HazardTime() {
            return Time.time - hazardBeganTime;
        }

        /* replaced with GameModel.Instance.SelectSailsForTargetting
        List<GameObject> GetTargetSails(int maxNumSelect = 1) {
            List<GameObject> targetSails = new List<GameObject>();
            List<GameObject> sails = GameModel.Instance.shipSails;
            bool respectCooldown = GameModel.Instance.lastSurvivingSailCount > maxNumSelect;
            Debug.Log("Selecting max " + maxNumSelect + " sails to break. Respect Cooldown: " + respectCooldown + ".");
            foreach (GameObject sail in sails)
            {
                Ship.SailsActivatable sailScript = sail.GetComponent<Ship.SailsActivatable>();
                Debug.Log("Sail " + sail.name + " is broken: " + sailScript.isBroken + " is cooldown " + sailScript.IsOnCooldown() +".");
                if (!sailScript.isBroken && (!sailScript.IsOnCooldown() || !respectCooldown))
                {
                    targetSails.Add(sail);
                }
            }
            Debug.Log("Found " + targetSails.Count + " valid sails to break.");
            while (targetSails.Count > maxNumSelect)
            {
                targetSails.RemoveAt(Random.Range(0, targetSails.Count));
            }
            Debug.Log("Selected " + targetSails.Count + " sails to break.");
            return targetSails;
        }
        */

        IEnumerator StartupSequence(float delay) {
            yield return new WaitForSeconds(delay-2f);
            GameModel.Instance.cameraController.ForceShipView(true);
            meteorPrompts.Sort((a, b) => a.priority.CompareTo(b.priority));
            foreach (UI.HelpPrompt meteorPrompt in meteorPrompts) {
                GameModel.Instance.helpPrompts.AddPrompt(meteorPrompt);
            }
            while (meteorsOut > 0) {
                yield return null;
            }
            yield return new WaitForSeconds(1.5f);
            GameModel.Instance.cameraController.ForceShipView(false);
        }

        IEnumerator EmitMeteors() {
            float nextMeteorSpawn = firstMeteorSpawnTimer;
            Debug.Log("Emit meteors started. First meteor in " + nextMeteorSpawn + " seconds.");
            while (!hasEnded) {
                float timeSinceGameBegan = HazardTime();
                if (timeSinceGameBegan > nextMeteorSpawn) {
                    meteorsOut = calcNextNumMeteors();
                    Debug.Log("Emitting " + meteorsOut + " meteors.");
                    List<GameObject> targetSails = GameModel.Instance.SelectSailsForTargetting(meteorsOut);
                    bool playSound = true;
                    foreach (GameObject randomSail in targetSails) {
                        randomSail.GetComponent<Ship.SailsActivatable>().TargetSail();
                        handleMeteorSpawning(randomSail, playSound);
                        playSound = false;
                    }
                    fullyRepairedIncreaseApplied = false;
                    nextMeteorSpawn = calcNextMeteorSpawnTime();
                    Debug.Log("Next meteor in " + (nextMeteorSpawn - HazardTime()) + " seconds.");
                } else if (meteorsOut == 0 && !fullyRepairedIncreaseApplied && GameModel.Instance.lastSurvivingSailCount == GameModel.Instance.shipSails.Count) {
                    
                    float remainingTimer = nextMeteorSpawn - timeSinceGameBegan;
                    nextMeteorSpawn = nextMeteorSpawn - (remainingTimer * fullyRepairedTimerReduction);
                    Debug.Log("All sails repaired, reducing time to next meteor by " + (remainingTimer*fullyRepairedTimerReduction));
                    fullyRepairedIncreaseApplied = true;
                }
                yield return null;
            }
        }

        void handleMeteorSpawning(GameObject targetSail, bool playSound)
        {
            float xPos = targetSail.transform.position.x + meteorSpawnXPos + Random.Range(-meteorSpawnXVariance, meteorSpawnXVariance);
            float yPos = targetSail.transform.position.y + meteorSpawnYPos + Random.Range(-meteorSpawnYVariance, meteorSpawnYVariance);
            GameObject meteorObject = Instantiate(meteorPrefab, new Vector2(xPos, yPos), Quaternion.identity);
            Meteorite meteor = meteorObject.GetComponent<Meteorite>();
            float delay = meteor.SetupMeteor(this, meteorSpeed, meteorObject.transform.position, targetSail, meteorSoundDuration, !playSound);   
            if (!startupSequence) {
                StartCoroutine(StartupSequence(delay));
                startupSequence = true;
            }
        }

        float calcNextMeteorSpawnTime() {
            float nextInterval = meteorInterval * (1 + Random.Range(-meteorIntervalVariance, meteorIntervalVariance));
            nextInterval = nextInterval*currentEscalationLevel.meteorIntervalMultiplier;
            return nextInterval + HazardTime();
        }

        int calcNextNumMeteors() {
            int survivingSails = GameModel.Instance.lastSurvivingSailCount;
            if (survivingSails == 1) {
                return 1;
            } else {
                int numMeteors = currentEscalationLevel.baseNumMeteors;
                if (survivingSails > 2 && numMeteors >= survivingSails) {
                    numMeteors = survivingSails -1;
                }
                return numMeteors;
            }
        }

        IEnumerator RockSpawner(GameObject emiter, float timeSinceGameBegan) {
            float nextRockSpawn = HazardTime() + firstRockSpawnTimer + Random.Range(0, rockVolleyBaseInterval);
            while (!hasEnded) {
                if (Time.time >= nextRockSpawn) {
                    float height = emiter.transform.position.y + Random.Range(-rockSpawnHeightVariance, rockSpawnHeightVariance);
                    GameObject rockObject = Instantiate(rockPrefab, new Vector2(emiter.transform.position.x, height), Quaternion.identity);
                    SpaceRock rock = rockObject.GetComponent<SpaceRock>();
                    float speed = (rockSpeed + currentEscalationLevel.rockSpeedIncrease)  * (1- Random.Range(-rockSpeedVariance, rockSpeedVariance));
                    float angle = rockAngleOffset + Random.Range(-rockAngleVariance, rockAngleVariance);
                    float scale = rockBaseSize * (1 + Random.Range(0, rockSizeIncreaseVariance));
                    bool sound = Random.Range(0, 100) < rockSoundChance;
                    rock.SetupRock(speed, angle, scale, sound);
                    nextRockSpawn = Time.time + rockVolleyBaseInterval / (GetCurrentRockRate(HazardTime()) * (1 - Random.Range(-rockVolleyIntervalVariance, rockVolleyIntervalVariance)));
                }
                yield return null;
            }
        }

        float GetCurrentRockRate(float timeSinceGameBegan) {
            float currentRockRate = baseRockRate;
            currentRockRate = currentRockRate + currentEscalationLevel.rockRateIncrease;
            return currentRockRate;
        }
        void FixedUpdate() {
            if (hazardBeganTime < 0) {
                return;
            } else if (hasEnded) {
                return;
            }
            float deltaTime = Time.fixedDeltaTime;
            float timeSinceStart = HazardTime();

            if (nextEscalationIndex < escalationLevels.Count && timeSinceStart > escalationLevels[nextEscalationIndex].timeIntoHazard) {
                Debug.Log("Escalating hazard " + this.gameObject.name + " to level " + nextEscalationIndex);
                currentEscalationLevel = escalationLevels[nextEscalationIndex];
                nextEscalationIndex++;
            }

            if (timeSinceStart > hazardDuration) {
                Debug.Log("Hazard " + this.gameObject.name + " has ended");
                hazardBeganTime = -1;
                hasEnded = true;
                return;
            }

            if (!meteorsStarted) {
                meteorsStarted = true;
                StartCoroutine(EmitMeteors());
            }


            if (!rocksStarted) {
                foreach (GameObject emiter in rockEmiters) {
                    StartCoroutine(RockSpawner(emiter, timeSinceStart));
                }
                rocksStarted = true;
            } 
        }

        public void StartHazard() {
            Debug.Log("Starting hazard " + this.gameObject.name);
            hazardDuration = baseDuration;
            hazardBeganTime = Time.time;
            hasEnded = false;
            meteorSoundDuration = SoundManager.Instance.Length("MeteorWhoosh_0");
            escalationLevels.Sort((a, b) => a.timeIntoHazard.CompareTo(b.timeIntoHazard));
        }

        void Start() {
            if (escalationLevels.Count == 0) {
                Debug.LogError("No escalation levels set for hazard " + this.gameObject.name);
                return;
            }
            if (testMode) {
                StartHazard();
            }
        }
    }
}