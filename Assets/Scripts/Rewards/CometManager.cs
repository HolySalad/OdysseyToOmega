using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Rewards {    
    public class CometManager : MonoBehaviour
    {
        [SerializeField] private GameObject cometPrefabDefault;
        [SerializeField] private Transform cometEmitterHigh;
        [SerializeField] private Transform cometEmitterLow;
        [SerializeField] private float cometSpawnYVariation = 3f;
        [SerializeField] private Transform cometTarget;
        [SerializeField] private float cometSpeed = 7f;
        [SerializeField] private float cometSpawnInterval = 30f;
        [SerializeField] private float cometSpawnIntervalVariationPercentage = 0.3f;

        [SerializeField] private float cometStartupDelay = 10f;
        [SerializeField] private int cometBurstCount = 3;
        [SerializeField] private float cometBurstInterval = 3.5f;
        [SerializeField] private float cometBurstIntervalVariationPercentage = 0.4f;


        private GameModel gameModel;
        private bool hasStarted = false;

        void Awake() {
            gameModel = FindObjectOfType<GameModel>();
        }

        void SpawnComet() {
            GameObject cometPrefab = cometPrefabDefault;
            Transform cometEmitter = cometEmitterHigh;
            if (Random.value < 0.5f) {
                cometEmitter = cometEmitterLow;
            }
            GameObject comet = Instantiate(cometPrefab, cometEmitter.position, Quaternion.identity);
            comet.GetComponent<RewardComet>().SetupComet(cometSpeed, cometTarget.position);
        }

        IEnumerator CometBurst() {
            for (int i = 0; i < cometBurstCount; i++) {
                float nextSpawnInterval = cometBurstInterval * (1 + Random.Range(-cometBurstIntervalVariationPercentage, cometBurstIntervalVariationPercentage));
                yield return new WaitForSeconds(nextSpawnInterval);
                SpawnComet();
            }
        }

        public void StartCometBurst() {
            StartCoroutine(CometBurst());
        }


        IEnumerator IntermittentCometSpawn() {
            yield return new WaitForSeconds(cometStartupDelay);
            while (true) {
                if (gameModel.hazardWindDown) {
                    yield return new WaitForSeconds(1);
                } else {
                    SpawnComet();
                    float nextSpawnInterval = cometSpawnInterval * (1 + Random.Range(-cometSpawnIntervalVariationPercentage, cometSpawnIntervalVariationPercentage));
                    yield return new WaitForSeconds(nextSpawnInterval);
                }
            }
        }

        public void StartCometSpawner() {
            hasStarted = true;
            StartCoroutine(IntermittentCometSpawn());
        }
    }
}