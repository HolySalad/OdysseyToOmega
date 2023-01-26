using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {
    public class CapturedState : MonoBehaviour, IPlayerState {
        public bool stealVelocityControl {get;} = true;

        private float releaseTimer = 0f;
        private Vector2 capturedVelocity = Vector2.zero;

        private Player player;

        public void SetReleaseTimer(float time) {
            releaseTimer = time;
        }

        public void SetCapturedVelocity(Vector2 velocity) {
            capturedVelocity = velocity;
        }

        public void EnterState(PlayerStateName previousState) {
            player = GetComponent<Player>();
            player.GetComponent<Rigidbody2D>().velocity = capturedVelocity;
        }
        public void ExitState(PlayerStateName nextState) {
            
        }

        public void UpdateState() {
            player.WalkInput(0f);
            player.CrouchInput(false);
            releaseTimer -= Time.deltaTime;
            if (releaseTimer <= 0) {
                player.ChangeState(PlayerStateName.ready);
            }
        }
    }
}