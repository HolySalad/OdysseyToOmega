using System.Collections;
using System.Collections.Generic;
using UnityEngine;

  namespace SpaceBoat.Items  {
    public class ClothItem : MonoBehaviour, IHeldItems
    {
        public ItemTypes itemType {get;} = ItemTypes.ClothItem;


        public string itemUsageValidTrigger {get;} = "Sails";
        public void ItemUsed(Player player, GameObject target) {
            target.GetComponent<Ship.SailsActivatable>().Repair();
        }

        public bool itemUsageCondition(Player player, GameObject target) {
            return target.GetComponent<Ship.SailsActivatable>().isBroken;
        }

        public string itemUsageSound {get;} = "Repair";
         public string usageAnimation {get;} = "Repairing";
        public bool isConsumed {get;} = true;

        public int usageFrames {get;} = 72;

        public bool currentlyHeld {get; set;} = false;
    }
}
