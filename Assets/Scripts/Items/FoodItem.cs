using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

  namespace SpaceBoat.Items {
    public class FoodItem : MonoBehaviour, IHeldItems
    {
        public string itemUsageValidTrigger {get;} = "Kitchen";
        public void ItemUsed(Player player, GameObject target) {
            player.PlayerHeals();
        }

        public bool isConsumed {get;} = true;
        public string itemUsageSound {get;} = "Cooking";
        public int usageFrames {get;} = 72;
    }
}