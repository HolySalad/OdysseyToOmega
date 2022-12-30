
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.UI;

namespace SpaceBoat.PlayerStates
{    
    public class AimingState : MonoBehaviour, IPlayerState
    {
        public static PlayerStateName playerState {get;} = PlayerStateName.aiming;
        public bool readyToTransition {get; private set;} = false;
        public PlayerStateName transitionState {get; private set;} = PlayerStateName.empty;

        //interface methods

        public void EnterState(Player player) {
            readyToTransition = false;
        }

        public void ExitState() {

            readyToTransition = false;
            transitionState = PlayerStateName.empty;
        }

        public void UpdateState() {
            
        }

        public void StateInput() {
            
        }

        public void HandleAddedPlayerMomentum(EntityMomentum momentum) {

        }

    }
}

