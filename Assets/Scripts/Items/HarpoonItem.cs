using System.Collections;
using System.Collections.Generic;
using UnityEngine;
  namespace SpaceBoat.Items{
    public class HarpoonItem : MonoBehaviour, IHeldItems
    {
        [SerializeField] public Sprite itemSprite {get;}
        [SerializeField] public Sprite encasedSprite {get;}

        public bool canBeUsed {get; private set;} = false;
        public bool isHeld {get; private set;} = false;
        private string interactionTag = "HarpoonGun";
        public string itemName {get;} = "Harpoon";
        public string helpText {get;} = "=";


        //TODO implement harpoon gun.
        public void Input()
        {
            throw new System.NotImplementedException();
        }

        public void HeldMode() {
            isHeld = true;
        }

        public void DropMode() {
            isHeld = false;
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