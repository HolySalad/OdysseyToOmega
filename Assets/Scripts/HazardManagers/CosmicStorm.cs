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
        [SerializeField] private float cloudSpeedVariation = 0.3f;
        
        [SerializeField] private float cloudSpawnInterval = 0.5f;
        [SerializeField] private float cloudSpawnYVariation = 1f;
        [SerializeField] private float cloudSizeVariation = 0.3f;

        [SerializeField] private float windSpeed = 6f;
        [SerializeField] private float windSpawnInterval = 0.5f;
        [SerializeField] private float lightningStrikeInterval = 1f;



        public int LightningStrikesPending { get; set; }
        
        public bool hasEnded { get; private set; }
        public float hazardDuration { get; private set; }

        private float lastCloudSpawnedTime = 0f;
        private float lastWindVolleyTime = 0f;
        private float lastLightningPendedTime = 0f;


        List<GameObject> clouds = new List<GameObject>();


        IEnumerator EmitWindVolley(Transform emitter) {
            yield break;
        }

        void EmitClouds(Transform emitter) {
            Vector3 spawnPosition = new Vector3 (
                emitter.position.x,
                emitter.position.y + Random.Range(-cloudSpawnYVariation, cloudSpawnYVariation),
                emitter.position.z
            );
            float size = Random.Range(1-cloudSizeVariation, 1+cloudSizeVariation);
            GameObject cloud = Instantiate(cloudPrefab, spawnPosition, Quaternion.identity);
            cloud.GetComponent<Rigidbody2D>().velocity = new Vector2(-cloudSpeed*(2-size)- cloudSpeed * Random.Range(1-cloudSizeVariation, 1+ cloudSizeVariation), 0f);
            cloud.transform.localScale = new Vector3(
                cloud.transform.localScale.x + Random.Range(-cloudSizeVariation, cloudSizeVariation),
                cloud.transform.localScale.y + Random.Range(-cloudSizeVariation, cloudSizeVariation),
                cloud.transform.localScale.z
            );

            clouds.Add(cloud);
            cloud.GetComponent<Cloud>().SetupCloud(this);
        }

        public void StartHazard()
        {
            hasEnded = false;
        }


        void FixedUpdate() {
            if (hasEnded) return;

            if (Time.time > lastLightningPendedTime + lightningStrikeInterval) {
                LightningStrikesPending++;
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


        }



    }
}
