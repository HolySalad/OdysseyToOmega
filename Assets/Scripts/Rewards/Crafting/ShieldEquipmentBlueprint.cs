using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.PlayerSubclasses.Equipment;
namespace SpaceBoat.Rewards {
    public class ShieldEquipmentBlueprint : MonoBehaviour, ICraftBlueprint
    {
        [SerializeField] private int cost = 100;
        [SerializeField] private RewardType rewardType = RewardType.ShieldEquipmentBlueprint;
        [SerializeField] private string title = "Shield";
        [SerializeField] private string subtitle = "Shield";
        [SerializeField] private string description = "Shield";
        [SerializeField] private Sprite iconSmall;
        [SerializeField] private Sprite iconLarge;

        public int Cost { get { return cost; } }
        public RewardType RewardType { get { return rewardType; } }

        public string Title { get { return title; } }
        public string Subtitle { get { return subtitle; } }
        public string Description { get { return description; } }
        public Sprite IconSmall { get { return iconSmall; } }
        public Sprite IconLarge { get { return iconLarge; } }
        public bool isUnlocked { get; set; } = false;

        public void Craft(Player player) {
            player.CraftEquipment(EquipmentType.Shield, cost);
        }

        public bool AlreadyOwns(Player player) {
            return player.HasEquipment(EquipmentType.Shield);
        }
    }
    
}