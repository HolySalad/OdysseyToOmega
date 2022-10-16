using System.Collections;
using System.Collections.Generic;
using UnityEngine;

  namespace SpaceBoat.Items  {
    public class ClothItem : MonoBehaviour, IHeldItems
    {
        public bool canBeUsed {get; private set;} = false;
        public bool isHeld {get; private set;} = false;
        private string interactionTag = "Sails";

        public Sprite itemSprite {get;}

        public string itemName {get;} = "Cloth";
        public string helpText {get;} = "=";
        public void Input()
        {
            throw new System.NotImplementedException();
        }

                void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag == interactionTag) {
                canBeUsed = true;
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.tag == interactionTag) {
                canBeUsed = false;
            }
        }

        public void HeldMode() {
            isHeld = true;
        }

        public void DropMode() {
            isHeld = false;
        }

    
                //TODO repair.

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
