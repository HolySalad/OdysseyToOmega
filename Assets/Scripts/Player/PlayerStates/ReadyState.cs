using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {
    public class ReadyState : MonoBehaviour, IPlayerState
    {
        public bool stealVelocityControl {get;} = false;

        private float timeEnteredState = 0;
        private bool jumpLockOut = false;
        private Player player;

        [SerializeField] private float jumpLockOutTime = 0.4f;
        [SerializeField] private float activatableLockoutTime = 0.4f;

        void Awake() {
            player = GetComponent<Player>();
        }

        public void JumpLockOut() {
            jumpLockOut = true;
        }

        public void EnterState(PlayerStateName previousState) {
           timeEnteredState = Time.time;
           // if we were previosuly aiming, we don't want the spacebar input 
           if (previousState == PlayerStateName.turret) {
                jumpLockOut = true;
           }

        }
        public void ExitState(PlayerStateName nextState) {
            
        }

        public void UpdateState() {
            bool isInActivtableLockout = Time.time < timeEnteredState + activatableLockoutTime;

            // Handle Input

            //Possibly state changing inputs.
            bool equipmentKeyHeld = isInActivtableLockout ? CthulkInput.EquipmentUsageKeyDown() : CthulkInput.EquipmentUsageKeyHeld();
            if (player.EquipmentUsageInput(CthulkInput.EquipmentUsageKeyDown(), equipmentKeyHeld)) return;
            bool activateInputHeld = isInActivtableLockout ? CthulkInput.ActivateKeyDown() : CthulkInput.ActivateKeyHeld();
            if (player.ActivateInput(activateInputHeld)) return;


            bool jumpKeyDown = CthulkInput.JumpKeyDown();
            bool jumpKeyHeld = CthulkInput.JumpKeyHeld();
            float horizontal = CthulkInput.HorizontalInput();
            bool crouchHeld = CthulkInput.CrouchHeld();

            player.CrouchInput(crouchHeld);

            player.WalkInput(horizontal);
            if (jumpLockOut && (jumpKeyDown || (Time.time > timeEnteredState + jumpLockOutTime))) {
                jumpLockOut = false;
            }
            player.JumpInput(jumpKeyHeld && !jumpLockOut, jumpKeyDown);

        }
    }
}