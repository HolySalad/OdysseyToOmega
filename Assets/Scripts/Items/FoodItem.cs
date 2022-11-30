using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

  namespace SpaceBoat.Items {
    public class FoodItem : MonoBehaviour, IHeldItems
    {
        public ItemTypes itemType {get;} = ItemTypes.FoodItem;
        public string itemUsageValidTrigger {get;} = "Kitchen";
        public void ItemUsed(Player player, GameObject target) {
            player.PlayerHeals();
        }

        public bool itemUsageCondition(Player player, GameObject target) {
            return player.health < player.maxHealth;
        }

        public bool isConsumed {get;} = true;
        public string itemUsageSound {get;} = "Cooking";
        public int usageFrames {get;} = 72;

        public bool currentlyHeld {get; set;} = false;
    }
}