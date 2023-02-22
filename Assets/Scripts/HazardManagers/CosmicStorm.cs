using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.HazardManagers.CosmicStormSubclasses;

namespace SpaceBoat.HazardManagers.CosmicStormSubclasses {
        [System.Serializable] public class EscalationLevel {
        public float timeIntoHazard = 0f;
        public int baseNumStrikes = 0;
        public float lightningStrikeIntervalMultiplier = 1f;
        public float windSpawnIntervalMultiplier = 0f;
        public float windSpeedIncrease = 0f;
    }

    [System.Serializable] public class EscalationSettings {
        public List<EscalationLevel> escalationLevelsEasy;
        public List<EscalationLevel> escalationLevelsMedium;
        public List<EscalationLevel> escalationLevelsHard;

        public List<EscalationLevel> GetEscalationLevels(HazardDifficulty difficulty) {
            switch (difficulty) {
                case HazardDifficulty.Easy:
                    return escalationLevelsEasy;
                case HazardDifficulty.Medium:
                    if (escalationLevelsMedium.Count == 0) return GetEscalationLevels(HazardDifficulty.Easy);
                    return escalationLevelsMedium;
                case HazardDifficulty.Hard:
                    if (escalationLevelsHard.Count == 0) return GetEscalationLevels(HazardDifficulty.Medium);
                    return escalationLevelsHard;
                default:
                    return escalationLevelsEasy;
            }
        }
    }
}

namespace SpaceBoat.HazardManagers {
   public class CosmicStorm : MonoBehaviour, IHazardManager
    {

        // constant spawns wind prefabs from the emitters; 
        // at an interval, strike sails with lightning;

        [SerializeField] private bool testMode = false;
        
        [Header("Prefabs")]
        [SerializeField] private GameObject[] windEmitters;
        [SerializeField] private GameObject cloudEmitter;
        [SerializeField] private GameObject cloudBorder;
        [SerializeField] private GameObject windPrefab;
        [SerializeField] private GameObject cloudPrefab;

        [Header("Escalation")]
        [SerializeField] private float baseHazardDuration = 145f;
        [SerializeField] private EscalationSettings escalationSettings;

        [Header("Cloud Settings")]
        [SerializeField] private float cloudSpeed = 1f;
        [SerializeField] private float cloudSpeedVariationPercentage = 0.3f;
        [SerializeField] private float cloudSpawnInterval = 0.5f;
        [SerializeField] private float cloudSpawnYVariation = 1f;
        [SerializeField] private float cloudSizeVariationPercentage = 0.3f;
        [SerializeField] private int cloudActiveInterval = 4;

        [Header("Wind Settings")]
        [SerializeField] private float windStartupDelay = 10f;
        [SerializeField] private float windSpeed = 6f;
        [SerializeField] private float windSpeedVariationPercentage = 0.3f;
        [SerializeField] private float windFirstSpawnVariationAbsolute = 4f;
        [SerializeField] private float windSpawnInterval = 1.5f;
        [SerializeField] private float windSpawnIntervalVariationAbsolute = 0.5f;
        [SerializeField] private float windSpawnYVariationAbsolute = 1f;

        [Header("Lightning Settings")]
        [SerializeField] private float lightningStrikeInterval = 25f; // TODO vary this based on num remaining sails
        [SerializeField] private int numStrikesAllSailsRepaired = 1;
        [SerializeField] private int numStrikesCriticalSailsRepaired = -1;
        [SerializeField] private float lightningFirstStrikeTime = 5f;
        [SerializeField] private float lightningBetweenStrikesBufferTime = 3f;


        private int nextEscalationIndex = 0;
        private EscalationLevel currentEscalationLevel;
        private List<EscalationLevel> escalationLevels;

        public int LightningStrikesPending { get; set; }
        private bool hasLightningStruck = false;
        private bool hasStartedWind = false;
        
        public HazardTypes hazardType {get;} = HazardTypes.CosmicStorm;
        public bool hasEnded { get; private set; }
        public bool wasCompleted { get; private set; } = false;
        public float hazardDuration { get; private set; }
        public string hazardSoundtrack { get; private set; } = "FirstGalaxy";

        private float lastCloudSpawnedTime = 0f;
        private float lastLightningPendedTime = 0f;
        private int activeCloudCounter = 0;
        private float hazardBeganTime = -1f;       
        float HazardTime() {
            return Time.time - hazardBeganTime;
        }


        List<GameObject> clouds = new List<GameObject>();


        IEnumerator EmitWindVolley(Transform emitter) {
            float nextWindSpawn = HazardTime() + Random.Range(0, windFirstSpawnVariationAbsolute);
            while (!hasEnded) {
                if (HazardTime() > nextWindSpawn) {
                    float yPos = emitter.position.y + Random.Range(-windSpawnYVariationAbsolute, windSpawnYVariationAbsolute);
                    GameObject wind = Instantiate(windPrefab, new Vector3(emitter.position.x, yPos, 0), Quaternion.identity);
                    float speedMult = Random.Range(1-windSpeedVariationPercentage, 1+windSpeedVariationPercentage); 
                    wind.GetComponent<Rigidbody2D>().velocity = new Vector2(-(windSpeed+currentEscalationLevel.windSpeedIncrease)*speedMult, 0f);
                    float currentWindSpawnInterval = windSpawnInterval * currentEscalationLevel.windSpawnIntervalMultiplier;
                    nextWindSpawn = Random.Range(currentWindSpawnInterval - windSpawnIntervalVariationAbsolute, currentWindSpawnInterval + windSpawnIntervalVariationAbsolute) + HazardTime();
                }
                yield return null;
            }
            yield break;
        }

        void EmitClouds(Transform emitter) {
            Vector3 spawnPosition = new Vector3 (
                emitter.position.x,
                emitter.position.y + Random.Range(-cloudSpawnYVariation, cloudSpawnYVariation),
                emitter.position.z
            );
            bool isActiveCloud = false;
            if (activeCloudCounter >= cloudActiveInterval) {
                activeCloudCounter = 0;
                isActiveCloud = true;
            } else {
                activeCloudCounter++;
            }
            float size = Random.Range(1-cloudSizeVariationPercentage, 1+cloudSizeVariationPercentage);
            GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);
            float actualCloudSpeed = isActiveCloud ? (-cloudSpeed) : (-cloudSpeed*(2-size) * Random.Range(1-cloudSpeedVariationPercentage, 1+ cloudSpeedVariationPercentage));
            cloud.GetComponent<Rigidbody2D>().velocity = new Vector2(actualCloudSpeed, 0f);
            float scaleModifier = Random.Range(-cloudSizeVariationPercentage, cloudSizeVariationPercentage);
            cloud.transform.localScale = new Vector3(
                cloud.transform.localScale.x + scaleModifier,
                cloud.transform.localScale.y + scaleModifier,
                cloud.transform.localScale.z
            );
            if (isActiveCloud) {
                clouds.Add(cloud);
                cloud.GetComponent<Cloud>().SetupCloud(this, 2);
            } else {
                cloud.GetComponent<Cloud>().SetupCloud(this, 1);
                cloud.GetComponent<Cloud>().enabled = false;
            }
            
        }

        public void StartHazard(HazardDifficulty difficulty) {
            escalationLevels = escalationSettings.GetEscalationLevels(difficulty);
            hasEnded = false;
            hazardBeganTime = Time.time;
        }

        void AddStrikes() {
            int totalSails = GameModel.Instance.shipSails.Count;
            int numSailsRepaired = GameModel.Instance.lastSurvivingSailCount;
            int numStrikes = currentEscalationLevel.baseNumStrikes;
            if (numSailsRepaired == totalSails) {
                numStrikes += numStrikesAllSailsRepaired;
            } else if (numSailsRepaired < totalSails/4) {
                numStrikes += numStrikesCriticalSailsRepaired;
            }
            LightningStrikesPending += numStrikes;
        }

        IEnumerator TriggerLightningStrikes() {
            while (!hasEnded) {
                float currentStrikeInterval = lightningStrikeInterval * currentEscalationLevel.lightningStrikeIntervalMultiplier;
                if (HazardTime() > lastLightningPendedTime + currentStrikeInterval) {
                    AddStrikes();
                    Debug.Log("Lightning strikes pending: " + LightningStrikesPending);
                    lastLightningPendedTime = HazardTime();
                }
                Dictionary<GameObject, bool> alreadyTargettedSails = new Dictionary<GameObject, bool>();
                if (LightningStrikesPending > 0) {
                    List<GameObject> cloudsToRemove = new List<GameObject>();
                    foreach (GameObject cloud in clouds) {
                        if (cloud == null || cloud.transform.position.x < cloudBorder.transform.position.x) {
                            cloudsToRemove.Add(cloud);
                        }
                    }
                    foreach (GameObject cloud in cloudsToRemove) {
                        clouds.Remove(cloud);
                    }
                    Debug.Log("Removed " + cloudsToRemove.Count + " clouds; " + clouds.Count + " clouds remain.");
                    cloudsToRemove.Clear();
                    // shuffle the list of clouds
                    for (int i = 0; i < clouds.Count; i++) {
                        GameObject temp = clouds[i];
                        int randomIndex = Random.Range(i, clouds.Count);
                        clouds[i] = clouds[randomIndex];
                        clouds[randomIndex] = temp;
                    }
                    List<GameObject> targetSails = GameModel.Instance.SelectSailsForTargetting(LightningStrikesPending);
                    Dictionary<GameObject, bool> targetSailsDict = new Dictionary<GameObject, bool>();
                    foreach (GameObject sail in targetSails) {
                        if (!alreadyTargettedSails.ContainsKey(sail)) 
                            {targetSailsDict.Add(sail, true);}
                    }
                    List<int> cloudsUsed = new List<int>();
                    for (int i = 0; i < clouds.Count; i++) {
                        if (LightningStrikesPending <= 0) break;
                        GameObject cloud = clouds[i];
                        if (cloud == null || cloud.GetComponent<Cloud>().isStriking) continue;
                        GameObject hitSail = cloud.GetComponent<Cloud>().CheckLightningStrike(targetSailsDict);
                        if (hitSail != null) {
                            LightningStrikesPending--;
                            targetSailsDict.Remove(hitSail);
                            alreadyTargettedSails.Add(hitSail, true);
                            hitSail.GetComponent<Ship.Activatables.SailsActivatable>().AddOnSailRepairCallback(
                                () => {
                                    alreadyTargettedSails.Remove(hitSail);
                                }
                            );
                            yield return new WaitForSeconds(lightningBetweenStrikesBufferTime);
                        }
                    }
                }
                yield return null;
            }
            yield break;
        }

        void Start() {
            hazardDuration = baseHazardDuration;
            if (escalationSettings.escalationLevelsEasy.Count == 0) {
                Debug.LogError("No escalation levels set for hazard " + this.gameObject.name);
                return;
            }
            if (testMode) {
                StartHazard(HazardDifficulty.Easy);
            }
        }


        void FixedUpdate() {
            if (hazardBeganTime < 0) {
                return;
            } else if (hasEnded) {
                return;
            }
            if (hasEnded) return;
            float timeSinceStart = HazardTime();
            if (timeSinceStart > hazardDuration) {
                hasEnded = true;
                return;
            }

            if (nextEscalationIndex < escalationLevels.Count && timeSinceStart > escalationLevels[nextEscalationIndex].timeIntoHazard) {
                Debug.Log("Escalating hazard " + this.gameObject.name + " to level " + nextEscalationIndex);
                currentEscalationLevel = escalationLevels[nextEscalationIndex];
                nextEscalationIndex++;
            }

            if (!hasLightningStruck) {
                if (timeSinceStart > lightningFirstStrikeTime) {
                    AddStrikes();
                    hasLightningStruck = true;
                    lastLightningPendedTime = HazardTime();
                    StartCoroutine(TriggerLightningStrikes());
                }
            } 

            if (HazardTime() > lastCloudSpawnedTime + cloudSpawnInterval) {
                EmitClouds(cloudEmitter.transform);
                lastCloudSpawnedTime = HazardTime();
            }

            if (!hasStartedWind && timeSinceStart > windStartupDelay) {
                foreach (GameObject emitter in windEmitters) {
                    StartCoroutine(EmitWindVolley(emitter.transform));
                }
                hasStartedWind = true;
                if (!SoundManager.Instance.IsPlaying("WindHowl")) {
                    SoundManager.Instance.Play("WindHowl");
                }
            }
        }
    }
}
