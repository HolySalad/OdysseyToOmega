using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship;

namespace SpaceBoat.PlayerStates {    
    public class LadderState : MonoBehaviour,  IPlayerState
    {

        [Header("Settings")]
        [SerializeField] private float ladderSpeed = 2f;
        [SerializeField] private float ladderCenteringSpeed = 2f;

        public bool stealVelocityControl {get;} = true;
        private LadderActivatable ladder;
        private Vector2 ladderDirectionVector = new Vector2(0,1);
        private float ladderAngle = 0f;


        private Player player;

        void Awake() {
            player = GetComponent<Player>();
        }

        public void EnterState(PlayerStateName previousState) {
            Debug.Log("Entered Ladder State");
            player.CrouchInput(false);
            if (player.activatableInUse is LadderActivatable) {
                ladder = (LadderActivatable) player.activatableInUse;
            } else {
                Debug.LogError("Player entered LadderState without a ladder activatable in use.");
            }
            (ladderDirectionVector, ladderAngle) = ladder.LadderDirection();
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("Ground"), true);
        }

        public void ExitState(PlayerStateName nextState) {
            Debug.Log("Exited Ladder State");
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("Ground"), false);
        }

        public void UpdateState() {
            if (player.ActivateInput(CthulkInput.ActivateKeyDown())) return;


            bool upHeld = CthulkInput.JumpKeyHeld();
            bool downHeld = CthulkInput.CrouchHeld();

            player.WalkInput(0);

            float centeringVelocity = 0f;
            if (Mathf.Abs(player.transform.position.x - ladder.transform.position.x) < 0.05f) {
                centeringVelocity = 0f;
            } else if (player.transform.position.x < ladder.transform.position.x) {
                centeringVelocity = ladderCenteringSpeed;
            } else if (player.transform.position.x > ladder.transform.position.x) {
                centeringVelocity = -ladderCenteringSpeed;
            }

            float velocityY = 0f;
            if (upHeld && downHeld) {

            } else if (upHeld) {
                // check if the player is in contact with the ladder's top exit point;
                // force them to jump off the ladder if they are.
                if (ladder.ExitInContactWithPlayer(player)) {
                    Debug.Log("Reached top of ladder");
                    player.ForceJump(true);
                    player.DetatchFromActivatable();
                    return;
                } else {
                    velocityY = ladderSpeed;
                }
            } else if (downHeld) {
                // force the player off the ladder if they are in contact with its bottom exit point.
                if (ladder.EntranceInContactWithPlayer(player)) {
                    Debug.Log("Reached bottom of ladder");
                    player.DetatchFromActivatable();
                    return;
                } else {
                    velocityY = -ladderSpeed;
                }
            }
            player.GetComponent<Rigidbody2D>().velocity = new Vector2(centeringVelocity, velocityY);
        }
    }
}