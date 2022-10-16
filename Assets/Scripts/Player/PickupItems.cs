using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;

namespace SpaceBoat.Player {
    public class PickupItems : MonoBehaviour
    {
        bool haveItem = false;
        public string itemInHand {get; private set;}
        private Transform itemPlace;
        private Transform originOverride;
        private Animator animator;
        private PlayerLogic player;
        private PlayerInput playerInput;
        [SerializeField] private float detectionRadius = 0.4f;
        [SerializeField] private GameObject clothPrefab;
        [SerializeField] private GameObject harpoonPrefab;
        [SerializeField] private GameObject foodPrefab;
        //TODO item prefab  

        //TODO create a array of prefab assets which can be picked up

        public bool canPickItem {get; set;} = true;


        void Awake(){
            itemPlace = transform.Find("ItemPlace").transform;
            originOverride = transform.Find("OriginOverride").transform;
            animator = GetComponent<Animator>();
            player = GetComponent<PlayerLogic>();
            playerInput = GetComponent<PlayerInput>();
        }

        public string GetItemType(GameObject item) {
            if (item.GetComponent<ClothItem>() != null) {
                return "ClothItem";
            } else if (item.GetComponent<HarpoonItem>() != null) {
                return "HarpoonItem";
            } else if (item.GetComponent<FoodItem>() != null) {
                return "FoodItem";
            }
            return "";
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

        public GameObject PrefabForItemType(string itemType) {
            if (itemType == "ClothItem") {
                return clothPrefab;
            } else if (itemType == "HarpoonItem") {
                return harpoonPrefab;
            } else if (itemType == "FoodItem") {
                return foodPrefab;
            }
            return null;
        }

        public GameObject DropItem() {
            itemPlace.GetComponent<SpriteRenderer>().sprite = null;
            animator.SetBool("HoldingObject", true);
            //TODO leave the item on the floor by re-creating the correct type of item from prefab
            
            haveItem = false;
            //TODO instantiate the item in the world
            GameObject droppedItem = Instantiate(PrefabForItemType(itemInHand), originOverride.position, Quaternion.identity);
            IHeldItems createdComponent = CreateItemComponent(droppedItem, itemInHand);
            createdComponent.DropMode();
            playerInput.heldItems = null; //unbind the input
            itemInHand = null;

            return droppedItem;
        }

        public void DropItem(bool destroy) {
            GameObject droppedItem = DropItem();
            if (destroy) {
                Destroy(droppedItem);
            }
        }



        public void GrabItem(GameObject item) {
            Vector3 itemScale = item.transform.localScale;
            itemPlace.localScale = itemScale;
            SpriteRenderer render = item.GetComponent<SpriteRenderer>();
            itemPlace.GetComponent<SpriteRenderer>().sprite = render.sprite;    //make the item a child so it follows the player
            animator.SetBool("HoldingObject", true);
            itemInHand = GetItemType(item);
            haveItem = true;
 
            Destroy(item);
            IHeldItems createdComponent = CreateItemComponent(player.gameObject, itemInHand);
            playerInput.heldItems = createdComponent;
            createdComponent.HeldMode();
            
        }

        private void CheckForItems(){
            Collider2D[] hits = Physics2D.OverlapCircleAll(originOverride.position, detectionRadius);
            foreach(Collider2D hit in hits){
                Debug.Log(hit.name);
                if(hit.gameObject.CompareTag("Collectable") && !haveItem){
                    GrabItem(hit.gameObject);
                    return;
                } else if(haveItem){
                    DropItem();
                    return;
                }
            }
        }



        // Update is called once per frame
        public void PickItem(bool pickItemDown)
        {
            if(canPickItem && pickItemDown){   //check if pressed e
                CheckForItems();
            }
        }
    }
}