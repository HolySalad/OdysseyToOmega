using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Items {
    public interface IHeldItems
    {
        public string itemUsageValidTrigger {get;}

        public string itemUsageSound {get;}
        public int usageFrames {get;}

        public bool isConsumed {get;}
        public void ItemUsed(Player player, GameObject target);


    }
}
