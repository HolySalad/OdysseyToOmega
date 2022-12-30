using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.UI;

namespace SpaceBoat.PlayerStates
{    
    public class ReadyState : MonoBehaviour, IPlayerState
    {
        [Header("Collision Detection Settings")]
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float slipCheckDistance = 0.2f;
        [SerializeField] private int groundCheckJumpMargin = 24; //how many frames after jumping to check for ground
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private float ceilingCheckDistance = 0.1f;
        [SerializeField] private Transform footCollider;
        [SerializeField] private Transform headCollider;
        [SerializeField] private Transform bodyCollider;
        [SerializeField] private Transform leftSlipCollider;
        [SerializeField] private Transform rightSlipCollider;
        [SerializeField] private Transform headSlipCollider;

        [Header("Jump Settings")]
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
        private bool isGrounded = false;
        private bool groundedOnHazard = false;

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

        void OnEnable() {
            player = GetComponent<Player>();
            if (player == null) {Debug.LogError("PlayerMovement: Player reference not found.");}
            cameraControls = Camera.main.GetComponent<CameraControls>();
            if (cameraControls == null) {Debug.LogError("PlayerMovement: CameraControls reference not found.");}
            animator = GetComponent<Animator>();
            if (animator == null) {Debug.LogError("PlayerMovement: Animator reference not found.");}
        }


        // walk script
        // handles horizontal movement and sprite orientation

        public void AdjustFacing(float horizontalInput) {
            if (horizontalInput > 0 && !isFacingRight || horizontalInput < 0 && isFacingRight) {
                isFacingRight = !isFacingRight;
                speed = -speed * turningSpeedMult;
                Vector3 existingColliderLocation = bodyCollider.transform.position;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                Vector3 colliderOffset =  existingColliderLocation - bodyCollider.transform.position;
                if (!isFacingRight) {cameraControls?.SetPlayerFocusXOffset(-colliderOffset.x) ;} else cameraControls?.SetPlayerFocusXOffset(0);
                transform.position = new Vector3 (transform.position.x + colliderOffset.x, transform.position.y + colliderOffset.y, transform.position.z);
            }
        }

        public void WalkInput(float horizontalInput, float deltaTime) {
            if (horizontalInput != 0) {lastHorizontal = horizontalInput;}
            // if we aren't pressing an input or if our speed is in excess of max walk speed while on the ground, we decelerate.
            if (horizontalInput == 0 || (Mathf.Abs(speed) > maxHoriontalVelocity && isGrounded)) {
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
                float castDirection = (speed*lastHorizontal > 0 ? 1 : -1);
                List<RaycastHit2D> hits = new List<RaycastHit2D>();
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(LayerMask.GetMask("Ground"));
                float numHits = bodyCollider.gameObject.GetComponent<Collider2D>().Cast(Vector2.right * castDirection, filter, hits, wallCheckDistance, true);
                if (numHits > 0) {
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
            if (player.playerState != PlayerStateName.ready) {return false;}
            return Time.frameCount < jumpGrace || isGrounded;
        }

        void updateJump(float deltaTime) {
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
                SoundManager sm = FindObjectOfType<SoundManager>();
                sm.Play("Jump"); 
                Debug.Log("JumpSquat > Jump");
                jumpSquat = false;
                isJumping = true;
                isGrounded = false;
                hitApex = false;
                currentVerticalForce = jumpPower;
                if (speed > maxWalkSpeed * jumpHorizontalSpeedWindow) {
                    speed = speed * jumpHorizontalMultiplier;
                }
            } else if (!isGrounded) {
                if (!hitApex) {
                    Debug.Log("Hit Apex after " + (Time.frameCount - jumpStartTime) + " frames");
                    hitApex = true;
                    //TODO jump animation > fall animation
                }
                if (isSlipping) {
                    currentVerticalForce = -slipSpeedVertical;
                } else if (targetVerticalMomentum <= 0) {
                    currentVerticalForce = Mathf.Max(-gravityTerminalVelocity, currentVerticalForce - gravityAcceleration * deltaTime);
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
            
            bool wasGrounded = isGrounded;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Ground", "PhysicalHazards"));
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            //RaycastHit2D hit = Physics2D.CircleCast(footCollider.position, footCollider.gameObject.GetComponent<Collider2D>().bounds.extents.x, new Vector3(0, -1, 0), groundCheckDistance, LayerMask.GetMask("Ground"));
            int hitCount = footCollider.gameObject.GetComponent<Collider2D>().Cast(new Vector3(0, -1, 0), filter, hits, groundCheckDistance, true);
            Debug.DrawRay(footCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(rightSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(leftSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(headSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            isGrounded = hitCount > 0;
            if (isGrounded) {
                groundedOnHazard = hits.TrueForAll(hit => hit.collider.gameObject.layer == LayerMask.NameToLayer("PhysicalHazards"));
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
                groundedOnHazard = false;
                if (wasGrounded) {
                    Debug.Log("Player left ground at " + Time.frameCount);
                } else {
                    RaycastHit2D hitRight = Physics2D.Raycast(rightSlipCollider.position, new Vector3(0, -1, 0), slipCheckDistance, LayerMask.GetMask("Ground"));
                    RaycastHit2D hitLeft = Physics2D.Raycast(leftSlipCollider.position, new Vector3(0, -1, 0), slipCheckDistance, LayerMask.GetMask("Ground"));
                    if (hitRight.collider != null && hitLeft.collider != null) {
                        Debug.Log("Player was wedged between two walls");
                        isJumping = false;
                        halfJump = false;
                        currentVerticalForce = 0;
                        isGrounded = true;
                    } else if (hitRight.collider != null) {
                        Debug.Log("Player slipped right");
                        isSlipping = true;
                        isSlippingLeft = true;
                       
                    } else if (hitLeft.collider != null) {
                        Debug.Log("Player slipped left");
                        isSlipping = true;
                        isSlippingLeft = false;
                    } else if (Physics2D.Raycast(headSlipCollider.position, new Vector3(0, -1, 0), slipCheckDistance, LayerMask.GetMask("Ground")).collider != null)
                    {

                    }
                }
            }
        }

        private void CheckHeadBump() {
            RaycastHit2D hit = Physics2D.Raycast(headCollider.position, Vector3.up, ceilingCheckDistance, LayerMask.GetMask("Ground"));
            Debug.DrawRay(footCollider.position, transform.TransformDirection(new Vector3(0, ceilingCheckDistance, 0)), Color.yellow, 0.1f);
            if (hit.collider != null) {
                Debug.Log("Ouch! My fucking head!");
                currentVerticalForce = Mathf.Min(0, currentVerticalForce);
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



        //accessors
        public bool GetIsGrounded() {
            return GetIsGrounded(true);
        }

        public  bool GetIsGrounded(bool includeHazards) {
            if (includeHazards) {
                return isGrounded || jumpGrace > 0;
            } else {
                return isGrounded;
            }
        }

        //interface methods

        public void EnterState(Player player) {
            readyToTransition = false;
        }

        public void ExitState(Player player) {

            readyToTransition = false;
            transitionState = PlayerStateName.empty;
        }

        public void UpdateState(Player player) {
            
        }
    }
}