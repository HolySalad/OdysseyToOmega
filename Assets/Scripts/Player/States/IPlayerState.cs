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
        public void ExitState();
        public void UpdateState();
        public void StateInput();

        public void HandleAddedPlayerMomentum(EntityMomentum momentum);

    }
}