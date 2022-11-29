using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

  namespace SpaceBoat.Items {
    public class FoodItem : MonoBehaviour, IHeldItems
    {
        public string itemUsageValidTrigger {get;} = "Kitchen";
        public void ItemUsed(Player player) {
            player.PlayerHeals();
        }

        public bool isConsumed {get;} = true;

        public int usageFrames {get;} = 72;
    }
}