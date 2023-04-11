using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers.CosmicStormSubclasses {
    public class Lightning : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag == "Player") {
                GameModel.Instance.player.TakeDamage();
            }
        }
    }
}