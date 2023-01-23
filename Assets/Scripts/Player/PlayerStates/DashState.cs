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
        
        private float currentDashSpeed = 0f;
        private float dashDirection = 0f;
        private bool reachedSpeed = false;
        private int frameEnteredState = 0;

        void Awake() {
            player = GetComponent<Player>();
            dashEquipment = player.GetComponent<DashEquipment>();
            rb = player.GetComponent<Rigidbody2D>();
        }

        public void EnterState(PlayerStateName previousState) {
            dashDirection = player.GetFacingDirection();
            currentDashSpeed = 0f;
            reachedSpeed = false;
            frameEnteredState = Time.frameCount;
        }
        public void ExitState(PlayerStateName nextState) {
            Debug.Log("Exited dash state at frame " + (Time.frameCount - frameEnteredState) + " with speed " + currentDashSpeed);
            player.OverrideWalkSpeed(currentDashSpeed * dashEquipment.maintainedMomentumMultiplier);
        }
        public void UpdateState() {
            player.WalkInput(dashDirection);
            if (reachedSpeed) {
                currentDashSpeed = Mathf.Max(currentDashSpeed - ( dashEquipment.dashDeceleration * Time.deltaTime), 0f);
            } else {
                currentDashSpeed = Mathf.Min(currentDashSpeed + ( dashEquipment.dashAcceleration * Time.deltaTime), dashEquipment.dashSpeed);
            }
            if (currentDashSpeed >= dashEquipment.dashSpeed) {
                reachedSpeed = true;
                Debug.Log("Reached speed at frame " + (Time.frameCount -frameEnteredState ));
            }
            rb.velocity = new Vector2(currentDashSpeed * dashDirection, 0);
        }
    }
}