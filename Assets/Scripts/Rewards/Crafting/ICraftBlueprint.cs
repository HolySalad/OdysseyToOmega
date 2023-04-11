using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Rewards {    
    public enum BlueprintType {Buildable, Equipment}
    public interface ICraftBlueprint
    {
        public int Cost { get; }
        public RewardType RewardType { get; }
        public bool isUnlocked { get; set; }
        public BlueprintType BlueprintType { get; }

        public void Craft(Player player);
        public bool AlreadyOwns(Player player);

        public string Title { get; }
        public string Subtitle { get; }
        public string Description { get; }
        public string FurtherDescription { get; }
        public Sprite IconSmall { get; }
        public Sprite IconLarge { get; }
    }
}