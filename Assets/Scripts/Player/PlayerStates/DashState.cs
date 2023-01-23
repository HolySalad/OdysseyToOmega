using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {    
    public class DashState : MonoBehaviour, IPlayerState
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

        }
    }
}