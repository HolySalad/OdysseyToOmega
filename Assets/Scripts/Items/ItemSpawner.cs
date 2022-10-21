using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpaceBoat.Items {
    public class ItemSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private float spawnRate = 15;

        private float lastSpawnedAt = 0f;
        private float itemLeftAt = 0f;
        private bool firstSpawn = false;
        private GameObject itemHeld;
        void respawnItem() {
            itemHeld = Instantiate(itemPrefab, transform.position, Quaternion.identity);
        }

        void FixedUpdate() {
            if (!firstSpawn) {
                respawnItem();
                firstSpawn = true;
            } else {
                if (itemHeld == null && Time.time > spawnRate + lastSpawnedAt && Time.time > spawnRate + itemLeftAt) {
                    respawnItem();
                    lastSpawnedAt = Time.time;
                }
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.tag == "Collectable") {
                itemLeftAt = Time.time;
            }
        }
    }
}