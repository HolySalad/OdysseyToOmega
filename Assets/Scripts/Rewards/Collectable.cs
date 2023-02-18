using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Rewards {
    public class Collectable : MonoBehaviour
    {
        [SerializeField] private RewardType rewardType;
        [SerializeField] private int value = 1;

        void Collect(Player player) {
            if (player == null) return;
            switch (rewardType) {
                case RewardType.Money:
                    player.PlayerGainsMoney(value);
                    break;
                /*
                case RewardType.HealthPackEquipmentBlueprint:
                    FindObjectOfType<GameModel>().AddHealthPackEquipmentBlueprint();
                    break;
                case RewardType.HarpoonLauncherEquipmentBlueprint:
                    FindObjectOfType<GameModel>().AddHarpoonLauncherEquipmentBlueprint();
                    break;
                case RewardType.DashEquipmentBlueprint:
                    FindObjectOfType<GameModel>().AddDashEquipmentBlueprint();
                    break;
                case RewardType.ShieldEquipmentBlueprint:
                    FindObjectOfType<GameModel>().AddShieldEquipmentBlueprint();
                    break;
                case RewardType.TrampolineActivatableBlueprint:
                    FindObjectOfType<GameModel>().AddTrampolineActivatableBlueprint();
                    break;
                case RewardType.HarpoonGunActivatableBlueprint:
                    FindObjectOfType<GameModel>().AddHarpoonGunActivatableBlueprint();
                    break;
                case RewardType.ShipShieldActivatableBlueprint:
                    FindObjectOfType<GameModel>().AddShipShieldActivatableBlueprint();
                    break;
                */
                default:
                    Debug.LogWarning("Collectable.cs: Collect() switch statement reached default case.");
                    break;
            }
            Destroy(gameObject);
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag == "Player") {
                Collect(other.gameObject.GetComponent<Player>());
            }
        }
    }
}