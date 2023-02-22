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
                default:
                    GameModel.Instance.saveGame.rewardsUnlocked[rewardType] = true;
                    //TODO trigger unlock UI.
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