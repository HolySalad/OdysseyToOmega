using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {
    public class Wind : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D collision) {
            bool isMapBounds = collision.gameObject.layer == LayerMask.NameToLayer("MapBounds");
            bool isShip = collision.gameObject.tag == "Ground" && collision.gameObject.CompareTag("Ship");
            if (isMapBounds || isShip) {
                Destroy(this.gameObject);
            }
        }
    }
}