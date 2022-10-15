using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LittleDeath.Player {
    public class pickupItems : MonoBehaviour
    {
        bool haveItem = false;
        private GameObject item;
        public string itemInHand {get; private set;}
        private Transform itemPlace;

        public bool canPickItem {get; set;} = true;

        void Awake(){
            itemPlace = transform.Find("ItemPlace").transform;
        }

        public void DropItem(){
            itemPlace.GetComponent<SpriteRenderer>().sprite = null;
            //TODO use the object on something, you have the name of the object on itemInHand
            itemInHand = null;
            haveItem = false;
        }

        public void GrabItem() {
            itemPlace.GetComponent<SpriteRenderer>().sprite = item.GetComponent<SpriteRenderer>().sprite;    //make the item a child so it follows the player
            itemInHand = item.name;
            haveItem = true;
        }



        // Update is called once per frame
        public void PickItem(bool pickItemDown)
        {
            if(canPickItem && pickItemDown){   //check if pressed e
                if(haveItem){   //Do i have an item?
                    DropItem();
                }
                else if(item != null && item.gameObject.CompareTag("Collectable")){ //TODO mark as collectable any items that you can collect
                    GrabItem();
                }
            }
        }
        
        void OnTriggerEnter2D(Collider2D coll){
            if(coll.gameObject.CompareTag("Item")){
                item = coll.gameObject;
            }
        }
        void OnTriggerExit2D(Collider2D coll){
            item = null;
        }
    }
}