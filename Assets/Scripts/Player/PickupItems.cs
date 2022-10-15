using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Player {
    public class PickupItems : MonoBehaviour
    {
        bool haveItem = false;
        public string itemInHand {get; private set;}
        private Transform itemPlace;
        private Transform originOverride;
        [SerializeField] private float detectionRadius = 0.4f;

        public bool canPickItem {get; set;} = true;

        void Awake(){
            itemPlace = transform.Find("ItemPlace").transform;
            originOverride = transform.Find("OriginOverride").transform;
        }

        public void DropItem(){
            itemPlace.GetComponent<SpriteRenderer>().sprite = null;
            //TODO leave the item on the floor
            itemInHand = null;
            haveItem = false;
        }

        public void GrabItem(GameObject item) {
            itemPlace.GetComponent<SpriteRenderer>().sprite = item.GetComponent<SpriteRenderer>().sprite;    //make the item a child so it follows the player
            //TODO call animator and activate holding object bool
            itemInHand = item.name;
            haveItem = true;
        }

        private void CheckForItems(){
            Collider2D[] hits = Physics2D.OverlapCircleAll(originOverride.position, detectionRadius);
            foreach(Collider2D hit in hits){
                Debug.Log(hit.name);
                if(hit.gameObject.CompareTag("Collectable") && !haveItem){
                    GrabItem(hit.gameObject);
                    return;
                }else if(hit.gameObject.CompareTag("Interactable")){
                    //TODO use the function to repare shit
                    return;
                }else if(haveItem){
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