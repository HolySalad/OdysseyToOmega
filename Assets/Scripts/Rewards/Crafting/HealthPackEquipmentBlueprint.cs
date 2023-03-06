using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.PlayerSubclasses.Equipment;
namespace SpaceBoat.Rewards {
    public class HealthPackEquipmentBlueprint : MonoBehaviour, ICraftBlueprint
    {
        [SerializeField] private int cost = 100;
        [SerializeField] private RewardType rewardType = RewardType.HealthPackEquipmentBlueprint;

        [SerializeField] private string title = "Healthpack";
        [SerializeField] private string subtitle = "Healthpack";
        [SerializeField] private string description = "Healthpack";
        [SerializeField] private string furtherDescription = "Hold shift to use";
        [SerializeField] private Sprite iconSmall;
        [SerializeField] private Sprite iconLarge;
        
        public BlueprintType BlueprintType { get; } = BlueprintType.Equipment;
        public int Cost { get { return cost; } }
        public RewardType RewardType { get { return rewardType; } }

        public string Title { get { return title; } }
        public string Subtitle { get { return subtitle; } }
        public string Description { get { return description; } }
        public string FurtherDescription { get { return furtherDescription; } }
        public Sprite IconSmall { get { return iconSmall; } }
        public Sprite IconLarge { get { return iconLarge; } }

        public bool isUnlocked { get; set; } = false;

        public void Craft(Player player) {
            player.CraftEquipment(EquipmentType.HealthPack, cost);
        }

        public bool AlreadyOwns(Player player) {
            return player.HasEquipment(EquipmentType.HealthPack);
        }

        
    }
}