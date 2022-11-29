using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SpaceBoat.Items {
    public class ItemSpawner : MonoBehaviour
    {
        public enum itemTypes {ClothItem, FoodItem, HarpoonItem};
        [SerializeField] private itemTypes itemType;
        [SerializeField] private float spawnRate = 15;

        private float lastSpawnedAt = 0f;
        private float itemLeftAt = 0f;
        private bool firstSpawn = false;

        private GameModel game;

        void respawnItem() {
            //Instantiate(itemPrefab, transform.position, Quaternion.identity);
            GameObject item = Instantiate(game.PrefabForItemType(itemType.ToString()), transform.position, Quaternion.identity);
            game.CreateItemComponent(item, itemType.ToString());
        }

        void FixedUpdate() {
            if (!firstSpawn) {
                respawnItem();
                firstSpawn = true;
            }
        }

        void Awake() {
            game = FindObjectOfType<GameModel>();
        }
        
    }
}