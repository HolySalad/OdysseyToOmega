using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.PlayerSubclasses.Equipment;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {    
    public class DashState : MonoBehaviour, IPlayerState
    {
        public bool stealVelocityControl {get;} = true;
        private Player player;
        private DashEquipment dashEquipment;
        private Rigidbody2D rb;
        private Vector2 dashDirectionVector = Vector2.zero;
        private bool reachedSpeed = false;
        private int frameEnteredState = 0;
        private float dashVerticalMomentum = 0f;

        void Awake() {
            player = GetComponent<Player>();
            dashEquipment = player.GetComponent<DashEquipment>();
            rb = player.GetComponent<Rigidbody2D>();
            player.AddOnPlayerLandedCallback((Player player) => dashEquipment.hasLandedSinceLastDash = true);
            player.AddOnPlayerHeadbumpCallback(
                (Player player) => {
                    if (player.currentPlayerStateName == PlayerStateName.dash) {
                        player.DeactivateEquipment();
                    }
                }
            );
        }

        public void EnterState(PlayerStateName previousState) {
            float dashDirection = CthulkInput.HorizontalInput();
            float dashHeight = CthulkInput.JumpKeyHeld() ? 1f : 0f;
            if (dashDirection == 0f) {
                dashDirection = player.GetFacingDirection();
            }
            dashVerticalMomentum = 0f;
            if (dashHeight == 0f) {
                float playerVerticalVelocity = rb.velocity.y;
                dashVerticalMomentum = Mathf.Max(0, playerVerticalVelocity / dashEquipment.dashSpeed);
            } else {
                dashVerticalMomentum = 0.8f;
            }
            if (dashVerticalMomentum > 0f) dashVerticalMomentum = dashVerticalMomentum*0.5f;
            
            dashDirectionVector = new Vector2(dashDirection, dashVerticalMomentum).normalized;
            Debug.Log("Dashing at " + dashDirectionVector.ToString() + " at frame " + Time.frameCount);
            reachedSpeed = false;
            dashEquipment.hasLandedSinceLastDash = false;
            frameEnteredState = Time.frameCount;
        }
        public void ExitState(PlayerStateName nextState) {
            Debug.Log("Exited dash state at frame " + (Time.frameCount - frameEnteredState));
            player.OverrideWalkSpeed(Mathf.Abs(dashDirectionVector.x*dashEquipment.dashSpeed * dashEquipment.maintainedMomentumMultiplier));
            player.OverrideVerticalForce(dashDirectionVector.y*dashEquipment.dashSpeed * (dashEquipment.maintainedMomentumMultiplier/2));
            dashEquipment.hasLandedSinceLastDash = player.GetIsGrounded();
        }
        public void UpdateState() {
            if (dashDirectionVector.x != 0 && player.CheckWallBump(dashDirectionVector.x)) {
                player.DeactivateEquipment();
                return;
            }
            player.WalkInput(dashDirectionVector.x);
            rb.velocity = dashDirectionVector * dashEquipment.dashSpeed;
        }
    }
}