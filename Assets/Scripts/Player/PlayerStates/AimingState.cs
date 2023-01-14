using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerStates {
    public class AimingState : MonoBehaviour, IPlayerState
    {
        public bool stealVelocityControl {get;} = false;

        private int frameEnteredState = 0;
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
            player.ActivateInput(CthulkInput.ActivateKeyDown()); 
            // note, if we add more logic here, we should return if the above function returns true.
        }
    }
}