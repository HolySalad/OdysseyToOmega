using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.PlayerSubclasses.PlayerStates {
    public class StaticEquipmentState : MonoBehaviour, IPlayerState
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
            player.EquipmentUsageInput(CthulkInput.EquipmentUsageKeyDown(), CthulkInput.EquipmentUsageKeyHeld());
        }
    }
}