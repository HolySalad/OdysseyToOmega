using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.UI;

namespace SpaceBoat.PlayerStates
{    
    public class ReadyState : MonoBehaviour, IPlayerState
    {

        [Header("Jump Settings")]
        [SerializeField] private int groundCheckJumpMargin = 24; //how many frames after jumping to check for ground
        [SerializeField] private int jumpGraceWindow = 3;
        [SerializeField] private int halfJumpFrameWindow = 6;
        [SerializeField] private int jumpSquatFrames = 3;
        [SerializeField] private float halfJumpDecayMultiplier = 1.9f;
        [SerializeField] private float jumpPower = 22f;
        [SerializeField] private float jumpDecay = 25f;
        [SerializeField] private int jumpDecayDoublingFrames = 6;
        [SerializeField] private float gravityAcceleration = 56f;
        [SerializeField] private float slipSpeedVertical = 10f;
        [SerializeField] private float gravityTerminalVelocity = 72f;
        [SerializeField] private float jumpHorizontalMultiplier = 1.3f;
        [SerializeField] private float jumpHorizontalSpeedWindow = 0.75f;
        [SerializeField] private float landingHorizontalDrag = 0.7f;


        [Header("Walk Movement Settings")]
        [SerializeField] private float maxWalkSpeed = 8f;
        [SerializeField] private float maxHoriontalVelocity = 24f;

        [SerializeField] private float acceleration = 3f;
        [SerializeField] private float deceleration = 24f;
        [SerializeField] private float accelerationStartMult = 8f;
        [SerializeField] private float accelerationMidMult = 4f;
        [SerializeField] private float accelerationStartRange = 3f;
        [SerializeField] private float accelerationMidRange = 5f;
        [SerializeField] private float turningSpeedMult = 0.6f;
        
        [Header("Momentum Settings")]
        [SerializeField] private float momentumDecayHorizontal = 10f;
        [SerializeField] private float momentumDecayVertical = 15f;
        [SerializeField] private int momentumAccelerationTime = 12; //frames to reach max momentum

        //internal movement vars
        private int JumpGrace = 0;
        private int jumpGrace = 0;
        private bool  jumpSquat = false;
        private bool isJumping = false;
        private bool halfJump = false;
        private int jumpStartTime = 0;
        private float currentVerticalForce  = 0f;
        private bool hitApex = false;

        private int lastJumpStompFrame = 0;
        private int jumpStompCooldown = 18;

        private bool isFacingRight = true;
        private bool isWalking;
        private float lastHorizontal;
        private float speed;
        private float horizontalImpact;
        private bool justLanded = false;

        public bool isSlipping {get; private set;} = false;
        private bool isSlippingLeft = false;

        private float verticalMomentum = 0f;
        private float horizontalMomentum = 0f;
        private int momentumAddedOnFrame = 0;
        private float targetVerticalMomentum = 0f;
        private float targetHorizontalMomentum = 0f;

        // interface fields
        public static PlayerStateName playerState {get;} = PlayerStateName.ready;
        public PlayerStateName transitionState {get; private set;} = PlayerStateName.empty;
        public bool readyToTransition {get; private set;} = false;


        //references
        private Player player;
        private CameraControls cameraControls;
        private Animator animator;
        private Rigidbody2D rb;

        // walk script
        // handles horizontal movement and sprite orientation
        void AdjustFacing(float horizontalInput) {
            if (player.AdjustFacing(horizontalInput)) {
                speed = -speed * turningSpeedMult;
            }
        }
        
                

        public void WalkInput(float horizontalInput) {
            float deltaTime = Time.deltaTime;
            if (horizontalInput != 0) {lastHorizontal = horizontalInput;}
            // if we aren't pressing an input or if our speed is in excess of max walk speed while on the ground, we decelerate.
            if (horizontalInput == 0 || (Mathf.Abs(speed) > maxHoriontalVelocity && player.isGrounded)) {
                if (speed > 0) {
                    speed = Mathf.Max(speed - deceleration*deltaTime, 0);
                } else if (speed < 0) {
                    speed = Mathf.Min(speed + deceleration*deltaTime, 0);
                }
            } else  {
                float accel = acceleration;
                if (Mathf.Abs(speed) < accelerationStartRange) {
                    accel *= accelerationStartMult;
                } else if (Mathf.Abs(speed) < accelerationMidRange) {
                    accel *= accelerationMidMult;
                }
                //return speed after acceleration if it is higher than current speed.
                speed = Mathf.Max(Mathf.Min(speed + accel*deltaTime, maxWalkSpeed), speed);
            }

            //collisions
            if (isSlipping) {
                speed = (isSlippingLeft ? -1 : 1);
                isSlipping = false;
            } else if (speed != 0) {
                (bool walkingIntoWall, List<RaycastHit2D> hits) = player.CheckWall();
                if (walkingIntoWall) {
                    Debug.Log("Walking into a wall");
                    speed = 0;
                }
            }
            AdjustFacing(horizontalInput);
        }

        // jump functions

        private void StartJump() {
            if (CanJump()) {
                isJumping = true;
                jumpStartTime = Time.frameCount;
                jumpSquat = true;
                animator.SetTrigger("Jump");
            }
        }

        public void forceInstantHalfJump() {
            if (CanJump()) {
                isJumping = true;
                halfJump = true;
                jumpStartTime = Time.frameCount;
                jumpSquat = false;
                animator.SetTrigger("Jump");
            }
        }

        private bool CanJump() {
            return Time.frameCount < jumpGrace || player.isGrounded;
        }

        void updateJump() {
            if (currentVerticalForce > 0) {
                float decay = jumpDecay;
                if (halfJump) {
                    decay *= halfJumpDecayMultiplier;
                }
                if (Time.frameCount > jumpStartTime + jumpSquatFrames + jumpDecayDoublingFrames) {
                    decay *= 2;
                }
                currentVerticalForce = Mathf.Max(0, currentVerticalForce - decay * Time.deltaTime);
            } else if (jumpSquat && Time.frameCount > jumpStartTime + jumpSquatFrames) {
                SoundManager sm = FindObjectOfType<SoundManager>();
                sm.Play("Jump"); 
                Debug.Log("JumpSquat > Jump");
                jumpSquat = false;
                isJumping = true;
                player.LeavesGround();
                hitApex = false;
                currentVerticalForce = jumpPower;
                if (speed > maxWalkSpeed * jumpHorizontalSpeedWindow) {
                    speed = speed * jumpHorizontalMultiplier;
                }
            } else if (!player.isGrounded) {
                if (!hitApex) {
                    Debug.Log("Hit Apex after " + (Time.frameCount - jumpStartTime) + " frames");
                    hitApex = true;
                    //TODO jump animation > fall animation
                }
                if (isSlipping) {
                    currentVerticalForce = -slipSpeedVertical;
                } else if (targetVerticalMomentum <= 0) {
                    currentVerticalForce = Mathf.Max(-gravityTerminalVelocity, currentVerticalForce - gravityAcceleration * Time.deltaTime);
                }
            }
        }

        private void JumpInput(bool keyHeld, bool keyDown) {
            if ((keyDown || (justLanded && keyHeld)) && !isJumping) {
                StartJump();
            } else if (!keyHeld && isJumping && Time.frameCount < jumpStartTime + halfJumpFrameWindow) {
                Debug.Log("Half Jump");
                halfJump = true;
            }
            justLanded = false;
        }

        private void JumpStomp() {
            if (Time.frameCount > lastJumpStompFrame + jumpStompCooldown) {
                lastJumpStompFrame = Time.frameCount;
                float jumpStompVolume = 0.2f + (Mathf.Abs(currentVerticalForce/gravityTerminalVelocity) * 0.8f);
                Debug.Log("Jumpstomp with volume " + jumpStompVolume);
                SoundManager.Instance.Play("JumpStomp", jumpStompVolume);
            }
        }

        //collision functions

        private void CheckGrounded() {
            // if we are within a margin of starting a jump, we are still grounded
            // but we don't want to reset vertical momentum.
            if (Time.frameCount < jumpStartTime + groundCheckJumpMargin) {
                return;
            }
            (bool isGrounded, bool wasGrounded, List<RaycastHit2D> hits) = player.CheckGrounded();
            if (isGrounded) {
                JumpGrace = Time.frameCount + jumpGraceWindow;
                if (isJumping) {
                    Debug.Log("Player landed from jumping after " + (Time.frameCount - jumpStartTime) + " frames");
                    JumpStomp();
                    isJumping = false;
                    halfJump = false;
                    justLanded = true;
                    currentVerticalForce = 0;
                    targetVerticalMomentum = 0;
                    verticalMomentum = 0;
                    speed = speed * landingHorizontalDrag;
                } else if (!wasGrounded) {
                    Debug.Log("Player landed from falling after " + (Time.frameCount - jumpStartTime) + " frames");
                    JumpStomp();
                    isJumping = false;
                    halfJump = false;
                    justLanded = true;
                    currentVerticalForce = 0;
                    targetVerticalMomentum = 0;
                    verticalMomentum = 0;
                }
            } else {
                if (wasGrounded) {
                    Debug.Log("Player left ground at " + Time.frameCount);
                } else {
                    (bool slippingLeft, bool slippingRight) = player.CheckSlipColliders();
                    if (slippingLeft && slippingRight) {
                        Debug.Log("Player was wedged between two walls");
                        //give them cayote time so they can jump out of the wedge.
                        isJumping = false;
                        halfJump = false;
                        currentVerticalForce = 0;
                        jumpGrace = Time.frameCount + jumpGraceWindow;
                    } else if (slippingRight) {
                        Debug.Log("Player slipped right");
                        isSlipping = true;
                        isSlippingLeft = true;
                    } else if (slippingLeft) {
                        Debug.Log("Player slipped left");
                        isSlipping = true;
                        isSlippingLeft = false;
                    } 
                }
            }
        }

        // end of movement functions
        // momentum 

        public void AddMomentum(Vector2 momentum) {
            momentumAddedOnFrame = Time.frameCount;
            targetHorizontalMomentum = momentum.x;
            targetVerticalMomentum = momentum.y;
            Debug.Log("Player gained momentum " + targetHorizontalMomentum + ", " + targetVerticalMomentum);
        }


        void UpdateMomentum(float deltaTime) {
            if (Time.frameCount > momentumAddedOnFrame + momentumAccelerationTime && !(verticalMomentum == 0 && horizontalMomentum == 0)) {
                if (verticalMomentum != 0) verticalMomentum = Mathf.Max(0, Mathf.Abs(verticalMomentum)-(momentumDecayVertical*deltaTime)) * (verticalMomentum/Mathf.Abs(verticalMomentum));
                if (horizontalMomentum !=0) horizontalMomentum = Mathf.Max(0, Mathf.Abs(horizontalMomentum)-(momentumDecayHorizontal*deltaTime)) * (horizontalMomentum/Mathf.Abs(horizontalMomentum));
                Debug.Log("decelerating momentum to " + horizontalMomentum + ", " + verticalMomentum );
                if (verticalMomentum == 0 && horizontalMomentum == 0) {
                    Debug.Log("Player momentum stopped");
                    targetHorizontalMomentum = 0;
                    targetVerticalMomentum = 0;
                }
            } else if (Time.frameCount > momentumAddedOnFrame && !(targetHorizontalMomentum == 0 && targetVerticalMomentum == 0)) {
                float targetProportion = (Time.frameCount - momentumAddedOnFrame) / momentumAccelerationTime;
                if (Mathf.Abs(verticalMomentum) < Mathf.Abs(targetVerticalMomentum)*targetProportion) {
                    verticalMomentum = targetVerticalMomentum*targetProportion;
                }
                if (Mathf.Abs(horizontalMomentum) < Mathf.Abs(targetHorizontalMomentum)*targetProportion) {
                    horizontalMomentum = targetHorizontalMomentum*targetProportion;
                }
                Debug.Log("accelerating momentum stage ("+targetProportion+") to " + horizontalMomentum + ", " + verticalMomentum );
            }
        }

        void animatorUpdate() {
            if (playerState == PlayerStateName.ready) {
                animator.SetFloat("Speed", Mathf.Abs(speed));
                animator.SetBool("HoldingObject", (player.itemInHand != null));
            } else {
                animator.SetFloat("Speed", 0);
                animator.SetBool("HoldingObject", false);
            }
            animator.SetBool("Grounded", player.isGrounded);
        }


        //interface methods

        public void HandleAddedPlayerMomentum(EntityMomentum momentum) {

        }

        public void EnterState(Player player) {
            readyToTransition = false;
            this.player = player;
            cameraControls = Camera.main.GetComponent<CameraControls>();
            if (cameraControls == null) {Debug.LogError("PlayerMovement: CameraControls reference not found.");}
            animator = GetComponent<Animator>();
            if (animator == null) {Debug.LogError("PlayerMovement: Animator reference not found.");}
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) {Debug.LogError("PlayerMovement: Rigidbody2D reference not found.");}

            Debug.Log("Entered Ready State");
        }

        public void ExitState() {

            readyToTransition = false;
            transitionState = PlayerStateName.empty;
        }

        public void UpdateState() {
            CheckGrounded();
            if (isJumping) {
                //CheckHeadBump();
                //TODO add head bump
            }
            updateJump();
            float horizontal = lastHorizontal;

            Vector2 movement = new Vector2(Mathf.Min((speed*lastHorizontal)+horizontalMomentum, maxHoriontalVelocity), currentVerticalForce + verticalMomentum);
            if (playerState == PlayerStateName.working || playerState == PlayerStateName.aiming) {
                movement = new Vector2(0, Mathf.Min(currentVerticalForce, 0));
            } else if (playerState == PlayerStateName.hitstun) {
                //TODO add hitstun knockback
                movement = new Vector2(horizontalMomentum, Mathf.Min(currentVerticalForce, 0) + verticalMomentum);
            }
            rb.velocity = movement;

            animatorUpdate();
            //SoundUpdate();
        }

        public void StateInput() {
            bool jumpKeyDown = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
            bool jumpKeyHeld = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
            float horizontal = Input.GetAxisRaw("Horizontal");
            bool crouchHeld = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftControl);

            animator.SetBool("Crouching", crouchHeld);

            //Item pick up
            bool pickItemDown = Input.GetKeyDown(KeyCode.E);

            //Item Usage
            bool useItemDown = Input.GetKeyDown(KeyCode.Q); 

            WalkInput(horizontal);
            JumpInput(jumpKeyHeld, jumpKeyDown);
            //ItemInput(pickItemDown);
            //itemUsageInput(useItemDown);
        }
    }

}