using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Rewards {    

    public enum RewardType {
        Money,
        HealthPackEquipmentBlueprint,
        HarpoonLauncherEquipmentBlueprint,
        DashEquipmentBlueprint,
        ShieldEquipmentBlueprint,
        TrampolineActivatableBlueprint,
        HarpoonGunActivatableBlueprint, 
        ShipShieldActivatableBlueprint,
    }

    public class CometManager : MonoBehaviour
    {
        [SerializeField] private GameObject cometPrefabDefault;
        [SerializeField] private Transform cometEmitterHigh;
        [SerializeField] private Transform cometEmitterLow;
        [SerializeField] private Transform cometTarget;
        [SerializeField] private float cometSpeed = 7f;
        [SerializeField] private float cometSpawnInterval = 30f;
        [SerializeField] private float cometSpawnIntervalVariationPercentage = 0.3f;

        [SerializeField] private float cometStartupDelay = 10f;
        [SerializeField] private int cometBurstCount = 3;
        [SerializeField] private float cometBurstInterval = 3.5f;
        [SerializeField] private float cometBurstIntervalVariationPercentage = 0.4f;

        [SerializeField] private GameObject moneyPrefab;
        [SerializeField] private GameObject healthPackEquipmentBlueprintPrefab;
        [SerializeField] private GameObject harpoonLauncherEquipmentBlueprintPrefab;
        [SerializeField] private GameObject dashEquipmentBlueprintPrefab;
        [SerializeField] private GameObject shieldEquipmentBlueprintPrefab;
        [SerializeField] private GameObject trampolineActivatableBlueprintPrefab;
        [SerializeField] private GameObject harpoonGunActivatableBlueprintPrefab;
        [SerializeField] private GameObject shipShieldActivatableBlueprintPrefab;


        private GameModel gameModel;
        private bool hasStarted = false;

        void Awake() {
            gameModel = FindObjectOfType<GameModel>();
        }

        void SpawnComet() {
            GameObject cometPrefab = cometPrefabDefault;
            float yPos = Random.Range(cometEmitterLow.position.y, cometEmitterHigh.position.y);
            GameObject comet = Instantiate(cometPrefab, new Vector3(cometEmitterHigh.position.x, yPos, 0), Quaternion.identity);
            comet.GetComponent<RewardComet>().SetupComet(cometSpeed, cometTarget.position, moneyPrefab);
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
            if (hasStarted) {
                return;
            }
            hasStarted = true;
            StartCoroutine(IntermittentCometSpawn());
        }
    }
}