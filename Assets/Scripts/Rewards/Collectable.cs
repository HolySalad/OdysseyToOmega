using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Rewards {
    public class Collectable : MonoBehaviour
    {
        [SerializeField] private RewardType rewardType;
        [SerializeField] private int value = 1;
        [SerializeField] public string blueprintCollectableName = "Blueprint";

        void Collect(Player player) {
            if (player == null) return;
            switch (rewardType) { 
                case RewardType.Money:
                    player.GainMoney(value);
                    SoundManager.Instance.Play("MoneyPickup");
                    break;
                default:
                    GameModel.Instance.saveGame.rewardsUnlocked[rewardType] = true;
                    UI.UIManager.Instance.OpenBlueprintUnlockPanel(this);
                    SoundManager.Instance.Play("BlueprintPickup");
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