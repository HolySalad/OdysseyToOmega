using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerStates {
    public class HitstunState : MonoBehaviour, IPlayerState
    {
        public bool stealVelocityControl {get;} = false;

        private int frameEnteredState = 0;
        private Player player;
        
        [SerializeField] private int hitStunFrames = 24;

        void Awake() {
            player = GetComponent<Player>();
        }

        public void EnterState(PlayerStateName previousState) {
            frameEnteredState = Time.frameCount;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), true);
        }
        public void ExitState(PlayerStateName nextState) {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), false);
        }
        public void UpdateState() {
            if (Time.frameCount - frameEnteredState > hitStunFrames) {
                player.ChangeState(PlayerStateName.ready);
                return;
            }
            player.WalkInput(0f); // input 0 for walk movement to decelerate the player naturally.
        }
    }
}