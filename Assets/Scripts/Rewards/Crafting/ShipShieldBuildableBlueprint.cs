using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.PlayerSubclasses.Equipment;
namespace SpaceBoat.Rewards {
    public class ShipShieldBuildableBlueprint : MonoBehaviour, ICraftBlueprint
    {
        [SerializeField] private GameObject buildablePrefab;


        [SerializeField] private int cost = 100;
        [SerializeField] private RewardType rewardType = RewardType.ShipShieldBuildableBlueprint;
        [SerializeField] private string title = "Ship Shield";
        [SerializeField] private string subtitle = "Ship Upgrade";
        [SerializeField] private string description = "Place an activatable shield that can protect your ship for a short.";
        [SerializeField] private Sprite iconSmall;
        [SerializeField] private Sprite iconLarge;
        
        public BlueprintType BlueprintType { get; } = BlueprintType.Buildable;

        public int Cost { get { return cost; } }
        public RewardType RewardType { get { return rewardType; } }

        public string Title { get { return title; } }
        public string Subtitle { get { return subtitle; } }
        public string Description { get { return description; } }
        public Sprite IconSmall { get { return iconSmall; } }
        public Sprite IconLarge { get { return iconLarge; } }
        public bool isUnlocked { get; set; } = false;

        public void Craft(Player player) {
            UI.UIManager.Instance.EnterBuildMode(buildablePrefab, Cost);
        }

        public bool AlreadyOwns(Player player) {
            return player.HasEquipment(EquipmentType.Shield);
        }
    }
    
}