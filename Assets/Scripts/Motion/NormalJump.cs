using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Movement {
    public class NormalJump : MonoBehaviour, IMovementModifier, IJump
    {

        [Header("Jump Settings")]
        [SerializeField] private int jumpGraceWindow = 2;

        [SerializeField] private int halfJumpFrameWindow = 6;
        [SerializeField] private int jumpSquatFrames = 3;
        [SerializeField] private float halfJumpDecayMultiplier = 1.7f;
        [SerializeField] private float jumpPower = 22f;
        [SerializeField] private float jumpDecay = 28f;
        [SerializeField] private int jumpDecayDoublingFrames = 4;
        [SerializeField] private float gravityAcceleration = 30f;
        [SerializeField] private float gravityTerminalVelocity = 45f;

        private CharacterMotor motor;
        private Collider2D coll;
        private Animator animator;

        public int jumpGrace {get; private set;} = 0;
        public bool  jumpSquat {get; private set;} = false;
        public bool isJumping {get; private set;} = false;
        public bool halfJump {get; private set;} = false;
        public float jumpStartTime {get; private set;} = 0f;
        public float currentVerticalForce {get; private set;} = 0f;
        public bool isGrounded {get; private set;} = false;

        private bool hitApex = false;
        
        public void ReplaceJump(IMovementModifier other) {
            if (other is IJump) {
                IJump otherJump = (IJump)other;
                jumpGrace = otherJump.jumpGrace;
                jumpSquat = otherJump.jumpSquat;
                isJumping = otherJump.isJumping;
                halfJump = otherJump.halfJump;
                jumpStartTime = otherJump.jumpStartTime;
                currentVerticalForce = otherJump.currentVerticalForce;
                isGrounded = otherJump.isGrounded;
                other.OnDisable();
                UpdateModifier(Time.deltaTime);
                OnEnable();

            }
        }

        public Vector2 Value {get
            { return new Vector2(0, currentVerticalForce); }
        }

        public bool Enabled {get; private set;} = false;
        // on Awake, add references to rigidbody, CharacterMotor and collider
        void Awake()
        {
            // Get animator
            animator = this.gameObject.GetComponent<Animator>();
            motor = this.gameObject.GetComponent<CharacterMotor>();
            coll = this.gameObject.GetComponent<Collider2D>();
        }

        // add and remove movement modifiers when enabled or disabled
        // allows for turning movement behaviours on and off
        public void OnEnable()
        {
            Enabled = true;
        }
        public void OnDisable()
        {
            Enabled = false;
        }

        public void Input(bool keyDown) {
            if (keyDown && !isJumping) {
                StartJump();
            } else if (!keyDown && isJumping && Time.frameCount < jumpStartTime + halfJumpFrameWindow) {
                Debug.Log("Half Jump");
                halfJump = true;
            }
        }

        public void UpdateModifier(float deltaTime) {
            if (currentVerticalForce > 0) {
                float decay = jumpDecay;
                if (halfJump) {
                    decay *= halfJumpDecayMultiplier;
                }
                if (Time.frameCount > jumpStartTime + jumpSquatFrames + jumpDecayDoublingFrames) {
                    decay *= 2;
                }
                currentVerticalForce = Mathf.Max(0, currentVerticalForce - decay * deltaTime);
            } else if (jumpSquat && Time.frameCount > jumpStartTime + jumpSquatFrames) {
                jumpSquat = false;
                isJumping = true;
                hitApex = false;
                currentVerticalForce = jumpPower;
            } else if (!isGrounded) {
                if (!hitApex) {
                    Debug.Log("Hit Apex after " + (Time.frameCount - jumpStartTime) + " frames");
                    hitApex = true;
                }
                currentVerticalForce = Mathf.Max(-gravityTerminalVelocity, currentVerticalForce - gravityAcceleration * deltaTime);
            }
            //TODO send info to animator
        }

        private void StartJump() {
            if (CanJump()) {
                print("Clicked jump");
                isJumping = true;
                jumpStartTime = Time.frameCount;
                jumpSquat = true;
            }
        }

        private bool CanJump() {
                return Time.frameCount < jumpGrace || isGrounded;
            }

        void OnCollisionExit2D(Collision2D other) {
            // if the player leaves the ground, give them a grace period in which they can still jump.
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && !coll.IsTouchingLayers(LayerMask.GetMask("Ground"))) {
                //Debug.Log("Collision Exit from ground");
                jumpGrace = Time.frameCount + jumpGraceWindow;
                isGrounded = false;
            }
        }

        bool IsContactWithGroundFromAbove(Collision2D ground) {
            ContactPoint2D[] contacts = new ContactPoint2D[3];
            ground.GetContacts(contacts);
            foreach (ContactPoint2D contact in contacts) {
                //Debug.Log("Contact: " + contact.normal.y);
                if (contact.normal.y > 0.75f) {
                    return true;
                }
            }
            return false;
        }

        bool IsContactWithGroundFromBelow(Collision2D ground) {
            ContactPoint2D[] contacts = new ContactPoint2D[3];
            ground.GetContacts(contacts);
            foreach (ContactPoint2D contact in contacts) {
                //Debug.Log("Contact: " + contact.normal.y);
                if (contact.normal.y < -0.75f) {
                    return true;
                }
            }
            return false;
        }

        void OnCollisionEnter2D(Collision2D other) {
            // if the player lands on the ground, reset their jump
            if (!isGrounded && (other.gameObject.layer == LayerMask.NameToLayer("Ground"))) {
                //Debug.Log("Collision with ground");
                if (IsContactWithGroundFromAbove(other)) {
                    //Debug.Log("Collision with ground from above");
                    isGrounded = true;
                    isJumping = false;
                    halfJump = false;
                    currentVerticalForce = 0;
                } else if (IsContactWithGroundFromBelow(other)) {
                    //Debug.Log("Collision with ground from below");
                    currentVerticalForce = 0;
                }
            }
        }

        public void UpdateAnimator() {
            //TODO set animator state
        }
    }
}