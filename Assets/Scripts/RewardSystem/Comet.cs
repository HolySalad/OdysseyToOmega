using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;
using SpaceBoat.Player;


namespace SpaceBoat.RewardSystem {
    public class Comet : MonoBehaviour
    {
        private string heldItem;
        private bool isTargettingPity;
        private GameObject playerCharacter;
        private GameObject itemPrefab;

        GameObject[] FindGameObjectsInLayer(int layer){
            var goArray = FindObjectsOfType(typeof(GameObject)) as GameObject[];
            var goList = new System.Collections.Generic.List<GameObject>();
            for (int i = 0; i < goArray.Length; i++)
            {
                if (goArray[i].layer == layer)
                {
                    goList.Add(goArray[i]);
                }
            }
            if (goList.Count == 0)
            {
                return null;
            }
            return goList.ToArray();
        }

         public IHeldItems CreateItemComponent(GameObject target, string itemType) {
            if (itemType == "ClothItem") {
                return target.AddComponent<ClothItem>();
            } else if (itemType == "HarpoonItem") {
                return target.AddComponent<HarpoonItem>();
            } else if (itemType == "FoodItem") {
                return target.AddComponent<FoodItem>();
            }
            return null;
        }
        public void SetupComet(float speed, Vector3 startingPosition, GameObject target, GameObject itemPrefab, string itemType, GameObject player) {
            //define a vector from the starting position to the target sail
            heldItem = itemType;
            IHeldItems item = CreateItemComponent(this.gameObject, itemType);
            Vector2 targetVector = target.transform.position - startingPosition;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            //spriteRenderer.sprite = item.itemSprite;
            //TODO set the sprite for a child object;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(targetVector.normalized.x*speed, targetVector.normalized.y*speed);
            playerCharacter = player;
            this.itemPrefab = itemPrefab;
            isTargettingPity = target.tag == "CometPityTarget";
            
        }
        
        public void SpawnItemPayload(ContactPoint2D contactPoint2D) {
            Vector2 contactPoint = contactPoint2D.point;
            GameObject droppedItem = Instantiate(itemPrefab, contactPoint, Quaternion.identity);
            IHeldItems createdComponent = CreateItemComponent(droppedItem, heldItem);
            createdComponent.DropMode();
            Destroy(this.gameObject);
        }

        void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Comet OnTriggerEnter2D");
            if (other.gameObject.CompareTag("Harpoons")) {
                PullToShip();
                HandleHarpoonHitVisuals(other.gameObject);
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.CompareTag("CometTarget")) {
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground")) {
                SpawnItemPayload(collision.GetContact(0));
            }
        }
        
        void HandleHarpoonHitVisuals(GameObject harpoon) {

        }

        public void PullToShip() {
            GameObject target = FindGameObjectsInLayer(LayerMask.NameToLayer("CometPityTarget"))[0];
            Vector2 targetVector = target.transform.position - this.transform.position;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(targetVector.normalized.x*30, targetVector.normalized.y*30);
        }

        
    }
}