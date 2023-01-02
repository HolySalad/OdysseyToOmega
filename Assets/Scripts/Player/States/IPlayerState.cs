using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerStates {
    public interface IPlayerState
    {
        public static PlayerStateName playerState {get;}
        public bool readyToTransition {get;}
        public PlayerStateName transitionState {get;}

        public void EnterState(Player player);
        public void ExitState(Player player);
        public void UpdateState(Player player);

    }
}