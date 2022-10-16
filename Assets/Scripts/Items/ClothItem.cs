using System.Collections;
using System.Collections.Generic;
using UnityEngine;

  namespace SpaceBoat.Items  {
    public class ClothItem : MonoBehaviour, IHeldItems
    {
        [SerializeField] public Sprite itemSprite {get;}
        [SerializeField] public Sprite encasedSprite {get;}
        public bool canBeUsed {get; private set;} = false;
        public bool isHeld {get; private set;} = false;
        private string interactionTag = "Sails";
        private int repairingFrames = 72;
        private SpaceBoat.Sails sailScript;

        public string itemName {get;} = "Cloth";
        public string helpText {get;} = "=";

        void Repair() {
            Debug.Log("Player is repairing");
            this.gameObject.GetComponent<Animator>().SetBool("Repairing", true);
            FindObjectOfType<SoundManager>().Play("Repair"); 
            StartCoroutine(FinishRepair());
        }
        public void Input()
        {
            if (canBeUsed) {
                Repair();
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag == interactionTag) {
                sailScript = other.gameObject.GetComponent<SpaceBoat.Sails>();
                canBeUsed = true;
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (other.gameObject.tag == interactionTag) {
                sailScript = null;
                canBeUsed = false;
            }
        }

        public void HeldMode() {
            isHeld = true;
        }

        public void DropMode() {
            isHeld = false;
            canBeUsed = false;
        }

    
        IEnumerator FinishRepair() {
            yield return new WaitForSeconds(repairingFrames*Time.deltaTime);
            Debug.Log("Sail is repaired");
            sailScript.Repair();
            this.gameObject.GetComponent<Player.PlayerLogic>().SailRepairs();
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
