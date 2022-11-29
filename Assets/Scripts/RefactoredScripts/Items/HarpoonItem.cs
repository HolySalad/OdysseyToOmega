using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  namespace SpaceBoat.Items{
    public class HarpoonItem : MonoBehaviour, IHeldItems
    {
        public string itemUsageValidTrigger {get;} = "HarpoonGun";
        public void ItemUsed(Player player) {
            
        }

        public int usageFrames {get;} = 72;

    }
}