using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {
   public class CosmicStorm : MonoBehaviour, IHazardManager
    {

        // constant spawns wind prefabs from the emitters; 
        // at an interval, strike sails with lightning;


        [SerializeField] private GameObject[] windEmitters;
        [SerializeField] private GameObject cloudEmitter;
        [SerializeField] private GameObject cloudBorder;
        [SerializeField] private GameObject windPrefab;
        [SerializeField] private GameObject cloudPrefab;

        [SerializeField] private float cloudSpeed = 1f;
        [SerializeField] private float cloudSpeedVariationPercentage = 0.3f;
        
        [SerializeField] private float cloudSpawnInterval = 0.5f;
        [SerializeField] private float cloudSpawnYVariation = 1f;
        [SerializeField] private float cloudSizeVariationPercentage = 0.3f;

        [SerializeField] private float windStartupDelay = 10f;
        [SerializeField] private float windSpeed = 6f;
        [SerializeField] private float windSpeedVariationPercentage = 0.3f;
        [SerializeField] private float windFirstSpawnVariationAbsolute = 4f;
        [SerializeField] private float windSpawnInterval = 1.5f;
        [SerializeField] private float windSpawnIntervalVariationAbsolute = 0.5f;
        [SerializeField] private float windSpawnYVariationAbsolute = 1f;
        [SerializeField] private float lightningStrikeInterval = 25f; // TODO vary this based on num remaining sails
        [SerializeField] private int baseNumStrikes = 2;
        [SerializeField] private int numStrikesAllSailsRepaired = 1;
        [SerializeField] private int numStrikesCriticalSailsRepaired = -1;
        [SerializeField] private float lightningFirstStrikeTime = 5f;



        public int LightningStrikesPending { get; set; }
        private bool hasLightningStruck = false;
        private bool hasStartedWind = false;
        
        public bool hasEnded { get; private set; }
        public float hazardDuration { get; private set; }

        private float lastCloudSpawnedTime = 0f;
        private float lastLightningPendedTime = 0f;

        private float hazardBeganTime = -1f;


        List<GameObject> clouds = new List<GameObject>();


        IEnumerator EmitWindVolley(Transform emitter) {
            float nextWindSpawn = Time.time + Random.Range(0, windFirstSpawnVariationAbsolute);
            while (!hasEnded) {
                if (Time.time > nextWindSpawn) {
                    float yPos = emitter.position.y + Random.Range(-windSpawnYVariationAbsolute, windSpawnYVariationAbsolute);
                    GameObject wind = Instantiate(windPrefab, new Vector3(emitter.position.x, yPos, 0), Quaternion.identity);
                    float speedMult = Random.Range(1-windSpeedVariationPercentage, 1+windSpeedVariationPercentage); 
                    wind.GetComponent<Rigidbody2D>().velocity = new Vector2(-windSpeed*speedMult, 0f);
                    nextWindSpawn = Random.Range(windSpawnInterval - windSpawnIntervalVariationAbsolute, windSpawnInterval + windSpawnIntervalVariationAbsolute) + Time.time;
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
            float size = Random.Range(1-cloudSizeVariationPercentage, 1+cloudSizeVariationPercentage);
            GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);
            cloud.GetComponent<Rigidbody2D>().velocity = new Vector2(-cloudSpeed*(2-size) * Random.Range(1-cloudSpeedVariationPercentage, 1+ cloudSpeedVariationPercentage), 0f);
            cloud.transform.localScale = new Vector3(
                cloud.transform.localScale.x + Random.Range(-cloudSizeVariationPercentage, cloudSizeVariationPercentage),
                cloud.transform.localScale.y + Random.Range(-cloudSizeVariationPercentage, cloudSizeVariationPercentage),
                cloud.transform.localScale.z
            );

            clouds.Add(cloud);
            cloud.GetComponent<Cloud>().SetupCloud(this);
        }

        public void StartHazard()
        {
            hasEnded = false;
        }

        void AddStrikes() {
            int totalSails = GameModel.Instance.shipSails.Count;
            int numSailsRepaired = GameModel.Instance.lastSurvivingSailCount;
            int numStrikes = baseNumStrikes;
            if (numSailsRepaired == totalSails) {
                numStrikes += numStrikesAllSailsRepaired;
            } else if (numSailsRepaired < totalSails/4) {
                numStrikes += numStrikesCriticalSailsRepaired;
            }
            LightningStrikesPending += numStrikes;
        }


        void FixedUpdate() {
            if (hazardBeganTime < 0) {
                hazardBeganTime = Time.time;
            }
            if (hasEnded) return;

            if (!hasLightningStruck) {
                if (Time.time > lightningFirstStrikeTime+hazardBeganTime) {
                    AddStrikes();
                    hasLightningStruck = true;
                    lastLightningPendedTime = Time.time;
                }
            } else if (Time.time > lastLightningPendedTime + lightningStrikeInterval) {
                AddStrikes();
                Debug.Log("Lightning strikes pending: " + LightningStrikesPending);
                lastLightningPendedTime = Time.time;
                List<GameObject> cloudsToRemove = new List<GameObject>();
                foreach (GameObject cloud in clouds) {
                    if (cloud == null || cloud.transform.position.x < cloudBorder.transform.position.x) {
                        cloudsToRemove.Add(cloud);
                    }
                }
                Debug.Log("Removing " + cloudsToRemove.Count + " clouds");
                foreach (GameObject cloud in cloudsToRemove) {
                    clouds.Remove(cloud);
                    cloud.GetComponent<Cloud>().enabled = false;
                }
            }

            if (Time.time > lastCloudSpawnedTime + cloudSpawnInterval) {
                EmitClouds(cloudEmitter.transform);
                lastCloudSpawnedTime = Time.time;
            }

            if (!hasStartedWind && Time.time > windStartupDelay + hazardBeganTime) {
                foreach (GameObject emitter in windEmitters) {
                    StartCoroutine(EmitWindVolley(emitter.transform));
                }
                hasStartedWind = true;
            }
        }
    }
}
