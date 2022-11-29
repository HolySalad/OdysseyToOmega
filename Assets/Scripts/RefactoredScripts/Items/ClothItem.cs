using System.Collections;
using System.Collections.Generic;
using UnityEngine;

  namespace SpaceBoat.Items  {
    public class ClothItem : MonoBehaviour, IHeldItems
    {
        public string itemUsageValidTrigger {get;} = "Sails";
        public void ItemUsed(Player player) {

        }

        public bool isConsumed {get;} = true;

        public int usageFrames {get;} = 72;
    }
}
