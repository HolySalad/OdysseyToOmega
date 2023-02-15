using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {
    public class BallState : MonoBehaviour, IPlayerState
    {

        [SerializeField] float shieldGroundedDeceleration = 3f;
        [SerializeField] float shieldMaxSpeed = 15f;
        [SerializeField] float shieldMaxTorque = 180f;
        [SerializeField] float velocityToTorqueRatio = 12f;
        [SerializeField] float shieldTorqueDecay = 30f;

        public bool stealVelocityControl {get;} = true;
        private Player player;
        private Rigidbody2D rb;
        private HitstunState hitstunState;
        private GameObject shield;
        private float existingVerticalVelocity;
        private float existingHorizontalVelocity;
        private float currentTorque;

        void Awake() {
            player = GetComponent<Player>();
            rb = GetComponent<Rigidbody2D>();
            hitstunState = GetComponent<HitstunState>();
            shield = GetComponent<PlayerSubclasses.Equipment.ShieldEquipment>().shieldObject;
        }

        public void EnterState(PlayerStateName previousState) {
            existingVerticalVelocity = rb.velocity.y;
            existingHorizontalVelocity = rb.velocity.x;
            currentTorque = 0;
            rb.constraints = RigidbodyConstraints2D.None;
        }
        public void ExitState(PlayerStateName nextState) {
            if (nextState == PlayerStateName.hitstun) return;
            float rotation = transform.rotation.eulerAngles.z % 360;
            Debug.Log("Player exited shield state with rotation of " + rotation);
            if (rotation < 30 && rotation > -30) {
                rb.SetRotation(0);
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            } else {
                rb.SetRotation(0);
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                hitstunState.DontIgnoreCollisionOnNextHitstun();
                player.ChangeState(PlayerStateName.hitstun);
            }
        }

        bool shieldIsGrounded() {
            float dist = (shield.transform.localScale.y * shield.GetComponent<CircleCollider2D>().radius) + 0.1f;
            Debug.DrawLine(shield.transform.position, shield.transform.position + Vector3.down * dist, Color.red);
            //return Physics2D.Raycast(shield.transform.position, Vector2.down, dist, LayerMask.GetMask("Ground"));
            return false;
        }
        bool ShieldIsAgainstWall(float dir) {
            float dist = (shield.transform.localScale.x * shield.GetComponent<CircleCollider2D>().radius) + 0.1f;
            Vector3 offset = new Vector3(0, 0, 0);
            Debug.DrawLine(shield.transform.position - offset, shield.transform.position + Vector3.right * dist * dir, Color.yellow);
            //return Physics2D.Raycast(shield.transform.position - offset, Vector2.right * dir, dist, LayerMask.GetMask("Ground"));}
            return false;
        }

        public void UpdateState() {
            player.WalkInput(0f); // input 0 for walk movement to decelerate the player naturally.
            player.CrouchInput(false);
            if (player.EquipmentUsageInput(CthulkInput.EquipmentUsageKeyDown(), CthulkInput.EquipmentUsageKeyHeld())) return;
                existingVerticalVelocity = Mathf.Max(existingVerticalVelocity - (player.gravityAcceleration * Time.deltaTime), -player.gravityTerminalVelocity);
                if (shieldIsGrounded()) {
                    float velocitySign = Mathf.Sign(existingHorizontalVelocity);
                    if (ShieldIsAgainstWall(velocitySign)) {
                        existingHorizontalVelocity = 0;
                        currentTorque = 0;
                    } else {
                        if (CthulkInput.HorizontalInput() == velocitySign) {
                            existingHorizontalVelocity = velocitySign * Mathf.Min(Mathf.Abs(existingHorizontalVelocity) + (shieldGroundedDeceleration * Time.deltaTime), shieldMaxSpeed);
                        } else {
                            existingHorizontalVelocity = velocitySign * Mathf.Max(Mathf.Abs(existingHorizontalVelocity) - (shieldGroundedDeceleration * Time.deltaTime), 0);
                        }
                    }
                    if (Mathf.Abs(existingHorizontalVelocity) > 0) {
                        currentTorque = Mathf.Min(shieldMaxTorque, Mathf.Abs(existingHorizontalVelocity) * velocityToTorqueRatio) * -velocitySign;
                    } else {
                        float currentTorqueSign = Mathf.Sign(currentTorque);
                        currentTorque = currentTorqueSign * Mathf.Max(Mathf.Abs(currentTorque) - (shieldTorqueDecay * Time.deltaTime), 0);
                    }
                }
                rb.angularVelocity = currentTorque;
                rb.velocity = new Vector2(existingHorizontalVelocity, existingVerticalVelocity);
            }
        
    }
}