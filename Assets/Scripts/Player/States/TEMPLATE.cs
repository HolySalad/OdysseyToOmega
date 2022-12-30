/*
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.UI;

namespace SpaceBoat.PlayerStates
{    
    public class RENAME_THIS : MonoBehaviour, IPlayerState
    {
        public static PlayerStateName playerState {get;} = PlayerStateName.ready;
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
    }
}

*/