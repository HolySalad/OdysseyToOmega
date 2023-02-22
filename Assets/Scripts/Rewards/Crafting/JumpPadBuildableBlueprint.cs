using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.PlayerSubclasses.Equipment;
namespace SpaceBoat.Rewards {
    public class JumpPadBuildableBlueprint : MonoBehaviour, ICraftBlueprint
    {
        [SerializeField] private GameObject buildablePrefab;


        [SerializeField] private int cost = 100;
        [SerializeField] private RewardType rewardType = RewardType.JumpPadBuildableBlueprint;
        [SerializeField] private string title = "Jump Pad";
        [SerializeField] private string subtitle = "Ship Upgrade";
        [SerializeField] private string description = "Place a jump pad on the ship to quickly reach the sails.";
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

        private int numBuilt = 0;

        public void Craft(Player player) {
            UI.UIManager uim = UI.UIManager.Instance;
            uim.EnterBuildMode(buildablePrefab, Cost);
            uim.AddOnNextBuildModeExitCallback((bool isCancelled) => {
                if (isCancelled) return;
                Debug.Log("JumpPadBuildableBlueprint.Craft: numBuilt++");
                numBuilt++;
            });
        }

        public bool AlreadyOwns(Player player) {
            return numBuilt > 2;
        }
    }
    
}