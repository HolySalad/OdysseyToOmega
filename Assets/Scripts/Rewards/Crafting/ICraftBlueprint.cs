using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Rewards {    
    public interface ICraftBlueprint
    {
        public int Cost { get; }
        public RewardType RewardType { get; }
        public bool isUnlocked { get; set; }
        //TODO buildable activatables.

        public void Craft(Player player);
        public bool AlreadyOwns(Player player);

        public string Title { get; }
        public string Subtitle { get; }
        public string Description { get; }
        public Sprite IconSmall { get; }
        public Sprite IconLarge { get; }
    }
}