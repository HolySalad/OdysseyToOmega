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
        JumpPadBuildableBlueprint,
        HarpoonGunBuildableBlueprint, 
        ShipShieldBuildableBlueprint,
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

        [SerializeField] public GameObject moneyPrefab;
        [SerializeField] private GameObject healthPackEquipmentBlueprintPrefab;
        [SerializeField] private GameObject harpoonLauncherEquipmentBlueprintPrefab;
        [SerializeField] private GameObject dashEquipmentBlueprintPrefab;
        [SerializeField] private GameObject shieldEquipmentBlueprintPrefab;
        [SerializeField] private GameObject trampolineActivatableBlueprintPrefab;
        [SerializeField] private GameObject harpoonGunActivatableBlueprintPrefab;
        [SerializeField] private GameObject shipShieldActivatableBlueprintPrefab;

        [SerializeField] private int cometBlueprintDropChance = 50;
        [SerializeField] private int cometChanceReductionPerBlueprint = 10;
        [SerializeField] private int cometBlueprintDropChanceMin = 10;
        [SerializeField] private int maxMoneyDrop = 4;


        private GameModel gameModel;
        private bool hasStarted = false;

        void Awake() {
            gameModel = FindObjectOfType<GameModel>();
        }

        GameObject GetRewardPrefab(RewardType rewardType) {
            switch (rewardType) {
                case RewardType.Money:
                    return moneyPrefab;
                case RewardType.HealthPackEquipmentBlueprint:
                    return healthPackEquipmentBlueprintPrefab;
                case RewardType.HarpoonLauncherEquipmentBlueprint:
                    return harpoonLauncherEquipmentBlueprintPrefab;
                case RewardType.DashEquipmentBlueprint:
                    return dashEquipmentBlueprintPrefab;
                case RewardType.ShieldEquipmentBlueprint:
                    return shieldEquipmentBlueprintPrefab;
                case RewardType.JumpPadBuildableBlueprint:
                    return trampolineActivatableBlueprintPrefab;
                case RewardType.HarpoonGunBuildableBlueprint:
                    return harpoonGunActivatableBlueprintPrefab;
                case RewardType.ShipShieldBuildableBlueprint:
                    return shipShieldActivatableBlueprintPrefab;
                default:
                    return null;
            }
        }

        RewardType GetRandomRewardType() {
            int baseChance = cometBlueprintDropChance;
            List<RewardType> possibleRewards = new List<RewardType>();
            foreach (RewardType rewardType in GameModel.Instance.saveGame.rewardsUnlocked.Keys) {
                if (rewardType == RewardType.Money || GameModel.Instance.saveGame.rewardsUnlocked[rewardType] == true || GetRewardPrefab(rewardType) == null) {
                    continue;
                }
                possibleRewards.Add(rewardType);
                baseChance -= cometChanceReductionPerBlueprint;
            }
            Debug.Log("Possible rewards: " + possibleRewards.Count + " Chance: " + baseChance);
            if (possibleRewards.Count == 0 || Random.Range(0, 100) < Mathf.Max(baseChance, cometBlueprintDropChanceMin)) {
                return RewardType.Money;
            } else {
                return possibleRewards[Random.Range(0, possibleRewards.Count)];
            }
        }

        void SpawnComet() {
            GameObject cometPrefab = cometPrefabDefault;
            float yPos = Random.Range(cometEmitterLow.position.y, cometEmitterHigh.position.y);
            GameObject comet = Instantiate(cometPrefab, new Vector3(cometEmitterHigh.position.x, yPos, 0), Quaternion.identity);
            RewardType rewardType = GetRandomRewardType();
            GameObject rewardPrefab = GetRewardPrefab(rewardType);
            int secondaryItems = Random.Range(1, maxMoneyDrop);
            comet.GetComponent<RewardComet>().SetupComet(cometSpeed, cometTarget.position, rewardPrefab, moneyPrefab, secondaryItems);
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