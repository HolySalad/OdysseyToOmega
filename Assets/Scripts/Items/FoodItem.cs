using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  namespace SpaceBoat.Items {
    public class FoodItem : MonoBehaviour, IHeldItems
    {
        
        private int cookingFrames = 72;
        [SerializeField] public Sprite itemSprite {get;}
        [SerializeField] public Sprite encasedSprite {get;}

        public bool canBeUsed {get; private set;} = false;
        private string interactionTag = "Kitchen";
        public string itemName {get;} = "Food";
        public string helpText {get;} = "=";

        public bool isHeld {get; private set;} = false;

        public void HeldMode() {
            isHeld = true;
        }

        public void DropMode() {
            isHeld = false;
            canBeUsed = false;
        }



        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag == interactionTag) {
                Debug.Log("Food can be used");
                canBeUsed = true;
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.tag == interactionTag) {
                Debug.Log("Food can no longer be used");
                canBeUsed = false;
            }
        }

        void Cook() {
            Debug.Log("Player is cooking");
            this.gameObject.GetComponent<Animator>().SetBool("Repairing", true);
            StartCoroutine(FinishCook());
        }

        public void Input() {
            if (canBeUsed) {
                Cook();
            }
        }
    
        IEnumerator FinishCook() {
            yield return new WaitForSeconds(cookingFrames*Time.deltaTime);
            Debug.Log("Food is cooked");
            this.gameObject.GetComponent<Player.PlayerLogic>().PlayerHeals();
            this.gameObject.GetComponent<Player.PickupItems>().DropItem(true);
            this.gameObject.GetComponent<Animator>().SetBool("Repairing", false);
            this.gameObject.GetComponent<Animator>().SetTrigger("FinishedRepairing");
        }

        private bool GUIActive = false;
        void OnGUI() {
            if (canBeUsed) {
               GUIActive = true;
               //TODO help text
            } else {
               GUIActive = false;
               //TODO help text
            }
        }

    }
}