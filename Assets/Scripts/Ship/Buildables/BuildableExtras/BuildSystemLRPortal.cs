using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship.Buildables.BuildableExtras {    
    public class BuildSystemLRPortal : MonoBehaviour
    {
        [SerializeField] private BuildSystemLRPortal otherPortal;
        [SerializeField] public bool isLeftPortal;

        void Awake() {
            if (otherPortal.isLeftPortal == this.isLeftPortal) {
                Debug.LogError("Both portals "+this.name+ "/" + transform.parent.name +" & "+otherPortal.name+ "/" + otherPortal.transform.parent.name +" are the same type");
            }
        }

        public bool CheckTeleport(GameObject buildMarkerObject, bool isLeftMovement) {
            if (isLeftMovement != isLeftPortal) {
                return false;
            }
            if (otherPortal == null) {
                Debug.LogError("Portal "+this.name+ "/" + transform.parent.name +" has no other portal");
                return false;
            }
            buildMarkerObject.transform.position = otherPortal.transform.position;
            return true;
        }
    }
}