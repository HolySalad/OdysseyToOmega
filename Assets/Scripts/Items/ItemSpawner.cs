using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpaceBoat.Items {
    public class ItemSpawner : MonoBehaviour
    {

        [SerializeField] private ItemTypes itemType;
        [SerializeField] private float spawnRate = 15f;

        private float nextRespawnTime = 0;
        private bool firstSpawn = false;
        private bool waitingToSpawn = false;
        private GameObject spawnedItem;

        private GameModel game;

        void respawnItem() {
            //Instantiate(itemPrefab, transform.position, Quaternion.identity);
            GameObject item = Instantiate(game.PrefabForItemType(itemType), transform.position, Quaternion.identity);
            spawnedItem = item;
            game.CreateItemComponent(item, itemType);
            waitingToSpawn = false;
        }

        void FixedUpdate() {
            if (!firstSpawn) {
                respawnItem();
                firstSpawn = true;
            }
            if (spawnedItem == null && !waitingToSpawn) {
                Debug.Log("Respawning " + itemType.ToString() + " in " + spawnRate + " seconds");
                waitingToSpawn = true;
                nextRespawnTime = Time.time + spawnRate;
            }

            if (waitingToSpawn && Time.time > nextRespawnTime) {
                respawnItem();
            }
        }

        void Awake() {
            game = GameModel.Instance;
        }
        

    }
}