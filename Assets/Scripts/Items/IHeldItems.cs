using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

namespace SpaceBoat.Items {
    public interface IHeldItems
    {
        public string itemUsageValidTrigger {get;}
        public bool itemUsageCondition(Player player, GameObject target);

        public string itemUsageSound {get;}
        public int usageFrames {get;}

        public bool isConsumed {get;}
        public void ItemUsed(Player player, GameObject target);

        public ItemTypes itemType {get;}

        public bool currentlyHeld {get; set;}
    }
}
