using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {
    public interface IPlayerState {

        public bool stealVelocityControl {get;}
        public void EnterState(PlayerStateName previousState);
        public void ExitState(PlayerStateName nextState);
        public void UpdateState();

    }
}