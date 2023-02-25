using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {
    public class WorkingState : MonoBehaviour, IPlayerState
    {
        public bool stealVelocityControl {get;} = false;
        private Player player;
        

        void Awake() {
            player = GetComponent<Player>();
        }

        public void EnterState(PlayerStateName previousState) {

        }
        public void ExitState(PlayerStateName nextState) {

        }
        public void UpdateState() {
            player.WalkInput(0f); // input 0 for walk movement to decelerate the player naturally.
            player.CrouchInput(false);
            bool cancelActivation = CthulkInput.ActivateKeyDown();
            if (!CthulkInput.ActivateKeyHeld()) {
                if (CthulkInput.JumpKeyHeld()) cancelActivation = true;
                if (CthulkInput.HorizontalInputKeyDown()) cancelActivation = true;
                if (player.GetCurrentEquipmentScript().canCancelWorkToUse && CthulkInput.EquipmentUsageKeyDown()) cancelActivation = true;
            }
            if (cancelActivation) {
                player.JumpInput(CthulkInput.JumpKeyHeld(), CthulkInput.JumpKeyDown());
                player.WalkInput(CthulkInput.HorizontalInput());
                player.activatableInUse.Deactivate(player);
                player.DetatchFromActivatable();
                player.EquipmentUsageInput(CthulkInput.EquipmentUsageKeyDown(), CthulkInput.EquipmentUsageKeyHeld());
                return;
            }
            player.ActivateInput(CthulkInput.ActivateKeyDown()); 
            // note, if we add more logic here, we should return if the above function returns true.
        }
    }
}