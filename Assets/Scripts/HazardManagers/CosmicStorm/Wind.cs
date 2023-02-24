using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers.CosmicStormSubclasses {
    public class Wind : MonoBehaviour
    {
        [SerializeField] private bool isTestWind = false;

        void Start() {
            if (isTestWind) {
                GetComponent<Rigidbody2D>().velocity = new Vector2(-7.7f + Random.Range(-1, 1), 0);
            }
        }

        void OnTriggerEnter2D(Collider2D collision) {
            bool isMapBounds = collision.gameObject.layer == LayerMask.NameToLayer("MapBounds");
            bool isShip = collision.gameObject.layer == LayerMask.NameToLayer("Ground") && collision.gameObject.CompareTag("Ship");
            if (isMapBounds || isShip) {
                Destroy(this.gameObject);
            }
            bool isPlayer = collision.gameObject.tag == "Player" && collision.gameObject.GetComponent<Player>() != null;
            if (isPlayer) {
                collision.gameObject.GetComponent<Player>().ForceJump(false, true, true, 0.2f);
            }
        }
    }
}