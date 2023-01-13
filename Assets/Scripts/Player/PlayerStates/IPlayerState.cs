using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerStates {
    public interface IPlayerState {
        public bool stealInput {get;}
        public void EnterState(PlayerStateName previousState);
        public void ExitState(PlayerStateName nextState);
        public void UpdateState();

    }
}