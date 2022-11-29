using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  namespace SpaceBoat.Items {
    public class FoodItem : MonoBehaviour, IHeldItems
    {
        public string itemUsageValidTrigger {get;} = "Kitchen";
        public void ItemUsed(Player player) {
            
        }

        public int usageFrames {get;} = 72;
    }
}