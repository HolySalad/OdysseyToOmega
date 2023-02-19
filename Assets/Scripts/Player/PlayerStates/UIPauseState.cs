using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {
    public class UIPauseState : MonoBehaviour, IPlayerState
    {
        public bool stealVelocityControl {get;} = false;
        private Player player;
        
        private bool isActive = false;
        void Awake() {
            player = GetComponent<Player>();
        }

        public void EnterState(PlayerStateName previousState) {
            isActive = true;
        }
        public void ExitState(PlayerStateName nextState) {
            isActive = false;
        }
        public void UpdateState() {
            player.ChangeState(PlayerStateName.ready);
        }
    }
}