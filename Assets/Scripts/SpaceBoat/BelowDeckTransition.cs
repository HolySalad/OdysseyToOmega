using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Player;

namespace SpaceBoat.Environment {
    public class BelowDeckTransition : MonoBehaviour
    {
        void OnTriggerExit2D(Collider2D other) {
            Debug.Log("BelowDeckTransition OnTriggerEnter2D");
            int layer = other.gameObject.layer;
            if (layer == LayerMask.NameToLayer("PlayerChar")) {
                other.gameObject.GetComponent<PlayerLogic>().PlayerEntersOrExitsShip(transform);
            }
        }
    }
}