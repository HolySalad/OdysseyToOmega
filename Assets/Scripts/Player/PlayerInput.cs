using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;
using SpaceBoat.Movement;

namespace SpaceBoat.Player {
    public class PlayerInput : MonoBehaviour
    {
        public Movement.IJump jump {get; set;}
        public Movement.IWalk walk {get; set;}
        public Player.PickupItems pickupItems  {get; set;}

        public Items.IHeldItems heldItems {get; set;}

        private bool init = false;

        public void Init(IWalk walk, IJump jump) {
            init = true;
            this.walk = walk;
            this.jump = jump;
        }
 
        void Awake(){
            pickupItems = GetComponent<PickupItems>();
        }

        void Update() {
            if (!init) {
                print("Init was not called for the character controller!");
                return;
            }
            // get input
            bool jumpKeyDown = Input.GetKey(KeyCode.Space);
            float horizontal = Input.GetAxisRaw("Horizontal");

            //Item pick up
            bool pickItemDown = Input.GetKeyDown(KeyCode.E);

            //Item Usage
            bool useItemDown = Input.GetKeyDown(KeyCode.Q); 

            // apply input to movement
            walk.Input(horizontal);
            jump.Input(jumpKeyDown);
            pickupItems.PickItem(pickItemDown);

            if (heldItems != null && useItemDown) {
                heldItems.Input();
            }
        }

    }
}