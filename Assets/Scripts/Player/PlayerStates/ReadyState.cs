using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerStates {
    public class ReadyState : MonoBehaviour, IPlayerState
    {
        public bool stealInput {get;}

        private int frameEnteredState = 0;
        private bool jumpLockOut = false;
        private Player player;

        [SerializeField] private int jumpLockOutFrames = 6;
        

        void Awake() {
            player = GetComponent<Player>();
        }

        public void EnterState(PlayerStateName previousState) {
           frameEnteredState = Time.frameCount;
           // if we were previosuly aiming, we don't want the spacebar input 
           if (previousState == PlayerStateName.aiming) {
                jumpLockOut = true;
           }

        }
        public void ExitState(PlayerStateName nextState) {
            
        }
        public void UpdateState() {
            // Handle Input
            // Inputs which can change state go first.

                        //Item Usage
            if (player.ItemUsageInput(CthulkInput.UseItemDown())) return;
            if (player.ActivateInput(CthulkInput.ActivateKeyDown())) return;


            bool jumpKeyDown = CthulkInput.JumpKeyDown();
            bool jumpKeyHeld = CthulkInput.JumpKeyHeld();
            float horizontal = CthulkInput.HorizontalInput();
            bool crouchHeld = CthulkInput.CrouchHeld();

            player.animator.SetBool("Crouching", crouchHeld);

            //Item pick up
            bool pickItemDown = CthulkInput.PickItemDown();



            player.WalkInput(horizontal);
            if (jumpLockOut && (jumpKeyDown || (Time.frameCount > frameEnteredState + jumpLockOutFrames))) {
                jumpLockOut = false;
            }
            player.JumpInput(jumpKeyHeld && !jumpLockOut, jumpKeyDown);
            player.ItemInput(pickItemDown);

            // Camera Toggles
            if (CthulkInput.CameraToggleDown()) {
                player.cameraControls?.ToggleShipView();
            }
        }
    }
}