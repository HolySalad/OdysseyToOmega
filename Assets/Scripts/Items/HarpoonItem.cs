using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  namespace SpaceBoat.Items{
    public class HarpoonItem : MonoBehaviour, IHeldItems
    {
        public ItemTypes itemType {get;} = ItemTypes.HarpoonItem;
        public string itemUsageValidTrigger {get;} = "HarpoonGun";
        public void ItemUsed(Player player, GameObject target) {
            harpoonGun.LoadHarpoon();
        }
        public bool itemUsageCondition(Player player, GameObject target) {
            return !harpoonGun.isLoaded;
        }

        private Ship.HarpoonGun harpoonGun;

        public void Awake() {
            harpoonGun = FindObjectOfType<Ship.HarpoonGun>();
        }


        public bool isConsumed {get;} = true;
        public string itemUsageSound {get;} = "Repair";
         public string usageAnimation {get;} = "Repairing";
        public int usageFrames {get;} = 72;

        public bool currentlyHeld {get; set;} = false;
    }
}