using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship;
using SpaceBoat.PlayerStates;
using SpaceBoat.UI;

namespace SpaceBoat {
    public enum PlayerStateName {ready, working, hitstun, aiming, ladder, nullState};
    public class Player : MonoBehaviour
    {
        [Header("General Player Settings")]
        [SerializeField] private int invincibilityFrames = 50;
        [SerializeField] public int maxHealth = 3;

        [Header("Help Prompts")]
        [SerializeField] private HelpPrompt criticalHealthPrompt;

        [Header("Collision Detection Settings")]
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float slipCheckDistance = 0.2f;
        [SerializeField] private int groundCheckJumpMargin = 24; //how many frames after jumping to check for ground
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private float ceilingCheckDistance = 0.1f;
        [SerializeField] public Transform footCollider;
        [SerializeField] public Transform headCollider;
        [SerializeField] public Transform bodyCollider;
        [SerializeField] private Transform leftSlipCollider;
        [SerializeField] private Transform rightSlipCollider;
        [SerializeField] private Transform headSlipCollider;

        [Header("Jump Settings")]
        [SerializeField] private int jumpGraceWindow = 2;
        [SerializeField] private int jumpSquatFrames = 3;
        [SerializeField] private float jumpPower = 22f;
        [SerializeField] private float jumpDecay = 28f;
        [SerializeField] private int jumpDecayStartFrame = 4;
        [SerializeField] private float halfJumpEarliestFrame = 6;
        [SerializeField] private float gravityAcceleration = 30f;
        [SerializeField] private float slipSpeedVertical = 10f;
        [SerializeField] private float gravityTerminalVelocity = 45f;
        [SerializeField] private float jumpHorizontalMultiplier = 1.2f;
        [SerializeField] private float jumpHorizontalSpeedWindow = 0.5f;
        [SerializeField] private float landingHorizontalDrag = 0.7f;


        [Header("Walk Movement Settings")]
        [SerializeField] private float maxWalkSpeed = 6f;

        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float deceleration = 7f;
        [SerializeField] private float accelerationStartMult = 5f;
        [SerializeField] private float accelerationMidMult = 2f;
        [SerializeField] private float accelerationStartRange = 1f;
        [SerializeField] private float accelerationMidRange = 3f;
        [SerializeField] private float turningSpeedMult = 0.7f;
        [SerializeField] private float crouchedMovementMult = 0.4f;



        [Header("Momentum Settings")]
        [SerializeField] private float momentumDecayStartTime = 0.5f;
        [SerializeField] private float momentumDecayTime = 2f;
        [SerializeField] private float groundedMomentumDecayDivisor = 2f;
        //[SerializeField] private float momentumDecayHorizontal = 10f;
        //[SerializeField] private float momentumDecayVertical = 15f;
        //[SerializeField] private int momentumAccelerationTime = 12; //frames to reach max momentum

        // References
        private GameModel game;
        public Animator animator;
        private Rigidbody2D rb;
        private Collider2D playerLocationTrigger;
        public Transform itemPlace;
        private Transform originOverride;

        // player states
        //possible states: ready, working, hitstun
        public PlayerStateName currentPlayerStateName = PlayerStateName.ready;
        private IPlayerState currentPlayerState;
        private Dictionary<PlayerStateName, IPlayerState> playerStates = new Dictionary<PlayerStateName, IPlayerState>();


        // internal gameplay vars
        public int health {get; private set;}
        private int hitOnframe;

        //internal movement vars
        private bool isGrounded = false;
        private bool isCrouched = false;
        private GameObject groundedOnObject;
        public bool GetIsGrounded(bool includeJumpGrace = true, bool platformsAndShipOnly = false) {
            if (platformsAndShipOnly) {
                return (isGrounded || (includeJumpGrace && jumpGrace > 0)) &&
                (groundedOnObject != null && (groundedOnObject.tag == "Platforms" || groundedOnObject.tag == "Ship"));
            }
            return isGrounded || (includeJumpGrace && jumpGrace > 0);
        }


        private int jumpGrace = 0;
        private int frameLeftGround = 0;
        private bool  jumpSquat = false;
        private bool isJumping = false;
        private bool halfJump = false;
        private int jumpStartTime = 0;
        private float currentVerticalForce  = 0f;
        private bool hitApex = false;

        private int lastJumpStompFrame = 0;
        private int jumpStompCooldown = 18;

        private bool isFacingRight = true;
        private float lastHorizontalInput;
        private float currentWalkingSpeed;
        private bool justLanded = false;

        public bool isSlipping {get; private set;} = false;
        private bool isSlippingLeft = false;

        //imparted momentum
        
        private float currentHazardMomentum = 0f;
        private float lastAppliedHazardMomentum = 0f;
        private float lastMomentumAppliedTime = 0f;
        //private Dictionary<Environment.MomentumImpartingEffectKind, momentumEntry> momentumEntries = new Dictionary<Environment.MomentumImpartingEffectKind, momentumEntry>();


        //activatables

        public IActivatables activatableInUse {get; private set;}
        public float playerCameraXFocusOffset;

        void Awake() {
            //fill references
            game = FindObjectOfType<GameModel>();
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            itemPlace = transform.Find("ItemPlace").transform;
            originOverride = transform.Find("OriginOverride").transform;
            playerLocationTrigger = GetComponent<Collider2D>();

            //set default values
            health = maxHealth;


            //set up player states
            playerStates.Add(PlayerStateName.ready, GetComponent<ReadyState>() ?? gameObject.AddComponent<ReadyState>());
            playerStates.Add(PlayerStateName.hitstun, GetComponent<HitstunState>() ?? gameObject.AddComponent<HitstunState>());
            playerStates.Add(PlayerStateName.working, GetComponent<WorkingState>() ?? gameObject.AddComponent<WorkingState>());
            playerStates.Add(PlayerStateName.aiming, GetComponent<AimingState>() ?? gameObject.AddComponent<AimingState>());
            playerStates.Add(PlayerStateName.ladder, GetComponent<LadderState>() ?? gameObject.AddComponent<LadderState>());

            currentPlayerState = playerStates[currentPlayerStateName];
        }
        
        public void ChangeState(PlayerStateName newStateName) {
            if (currentPlayerStateName != newStateName && newStateName != PlayerStateName.nullState) {
                PlayerStateName oldStateName = currentPlayerStateName;
                currentPlayerStateName = newStateName;
                currentPlayerState = playerStates[newStateName];
                playerStates[oldStateName].ExitState(newStateName);
                currentPlayerState.EnterState(oldStateName);
            }
        }

        // walk script
        // handles horizontal movement and sprite orientation

        void AdjustFacing(float horizontalInput) {
            if (horizontalInput > 0 && !isFacingRight || horizontalInput < 0 && isFacingRight) {
                isFacingRight = !isFacingRight;
                currentWalkingSpeed = -currentWalkingSpeed * turningSpeedMult;
                Vector3 existingColliderLocation = bodyCollider.transform.position;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                Vector3 colliderOffset =  existingColliderLocation - bodyCollider.transform.position;
                if (!isFacingRight) {playerCameraXFocusOffset = -colliderOffset.x ;} else playerCameraXFocusOffset = 0;
                transform.position = new Vector3 (transform.position.x + colliderOffset.x, transform.position.y + colliderOffset.y, transform.position.z);
            }
        }

        public void Footfall() {

        }

        public void WalkInput(float horizontalInput) {
            float deltaTime = Time.deltaTime;
            if (horizontalInput != 0) {lastHorizontalInput = horizontalInput;}
            float maxSpeed = maxWalkSpeed;
            if (isCrouched && isGrounded) {maxSpeed *= crouchedMovementMult;}
            // if we aren't pressing an input or if our speed is in excess of max walk speed while on the ground, we decelerate.
            if (horizontalInput == 0 || (Mathf.Abs(currentWalkingSpeed) > maxSpeed && isGrounded)) {
                if (currentWalkingSpeed > 0) {
                    currentWalkingSpeed = Mathf.Max(currentWalkingSpeed - deceleration*deltaTime, 0);
                } else if (currentWalkingSpeed < 0) {
                    currentWalkingSpeed = Mathf.Min(currentWalkingSpeed + deceleration*deltaTime, 0);
                }
            } else  {
                float accel = acceleration;
                if (Mathf.Abs(currentWalkingSpeed) < accelerationStartRange) {
                    accel *= accelerationStartMult;
                } else if (Mathf.Abs(currentWalkingSpeed) < accelerationMidRange) {
                    accel *= accelerationMidMult;
                }
                //return speed after acceleration if it is higher than current speed.
                currentWalkingSpeed = Mathf.Max(Mathf.Min(currentWalkingSpeed + accel*deltaTime, maxSpeed), currentWalkingSpeed);
            }

            //collisions
            if (isSlipping) {
                currentWalkingSpeed = (isSlippingLeft ? -1 : 1);
                isSlipping = false;
            } else if (currentWalkingSpeed != 0) {
                if (CheckWallBump() && isGrounded) {
                    currentWalkingSpeed = 0;
                }
            }
            AdjustFacing(horizontalInput);
        }

        // jump functions

        private void StartJump(bool forceJump = false) {
            if (CanJump() || forceJump) {
                isJumping = true;
                jumpStartTime = Time.frameCount + jumpSquatFrames;
                jumpSquat = true;
                animator.SetTrigger("Jump");
            }
        }

        private bool CanJump() {
            return Time.frameCount < jumpGrace || isGrounded;
        }

        void updateJump(bool headBumped) {
            if (headBumped) {
                jumpSquat = false;
                currentVerticalForce = slipSpeedVertical*(1-landingHorizontalDrag);
            } else 
            if (currentVerticalForce > 0) {
                float decay = 0;
                if ((halfJump && Time.frameCount > jumpStartTime + halfJumpEarliestFrame) || (Time.frameCount > jumpStartTime + jumpDecayStartFrame)) {
                    decay = jumpDecay;
                }
                currentVerticalForce = Mathf.Max(0, currentVerticalForce - decay * Time.deltaTime);
            } else if (jumpSquat && Time.frameCount > jumpStartTime ) {
                SoundManager sm = FindObjectOfType<SoundManager>();
                sm.Play("Jump"); 
                Debug.Log("JumpSquat > Jump");
                jumpSquat = false;
                isJumping = true;
                isGrounded = false;
                hitApex = false;
                currentVerticalForce = jumpPower;
                if (currentWalkingSpeed > maxWalkSpeed * jumpHorizontalSpeedWindow) {
                    currentWalkingSpeed = currentWalkingSpeed * jumpHorizontalMultiplier;
                }
            } else if (!isGrounded) {
                if (!hitApex) {
                    Debug.Log("Hit Apex after " + (Time.frameCount - jumpStartTime) + " frames");
                    hitApex = true;
                    //TODO jump animation > fall animation
                }
                if (isSlipping) {
                    currentVerticalForce = -slipSpeedVertical;
                } else {
                    currentVerticalForce = Mathf.Max(-gravityTerminalVelocity, currentVerticalForce - gravityAcceleration * Time.deltaTime);
                }
            }
        }

        public void JumpInput(bool keyHeld, bool keyDown) {
            //skip one frame of input after landing.
            if (justLanded) {
                justLanded = false;
                return;
            }
            if ((keyHeld 
            //|| (justLanded && keyHeld)
            ) && !isJumping) {
                StartJump();
            } else if (!keyHeld && isJumping && !halfJump) {
                Debug.Log("Half Jump");
                halfJump = true;
            }
        }

        public void ForceJump(bool lockOutReadyState = false, bool halfJump = false, bool skipJumpSquat = false) {
            StartJump(true);
            if (skipJumpSquat) {
                jumpStartTime = Time.frameCount;
            }
            if (halfJump) {
                this.halfJump = true;
            }
            if (lockOutReadyState) {
                IPlayerState ready = playerStates[PlayerStateName.ready];
                if (ready is ReadyState) {
                    ((ReadyState)ready).JumpLockOut();
                }
            }
        }

        private void JumpStomp() {
            if (Time.frameCount > lastJumpStompFrame + jumpStompCooldown) {
                lastJumpStompFrame = Time.frameCount;
                float jumpStompVolume = 0.2f + (Mathf.Abs(currentVerticalForce/gravityTerminalVelocity) * 0.8f);
                Debug.Log("Jumpstomp with volume " + jumpStompVolume);
                SoundManager.Instance.Play("JumpStomp", jumpStompVolume);
            }
        }

        public void CrouchInput(bool crouchHeld) {
            if (crouchHeld) {
                isCrouched = true;
                animator.SetBool("Crouching", true);
            } else {
                isCrouched = false;
                animator.SetBool("Crouching", false);
            }
        }

        //collision functions

        private (bool, bool, List<RaycastHit2D>) CheckGrounded() {
            bool wasGrounded = isGrounded;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Ground"));
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            //RaycastHit2D hit = Physics2D.CircleCast(footCollider.position, footCollider.gameObject.GetComponent<Collider2D>().bounds.extents.x, new Vector3(0, -1, 0), groundCheckDistance, LayerMask.GetMask("Ground"));
            int hitCount = footCollider.gameObject.GetComponent<Collider2D>().Cast(new Vector3(0, -1, 0), filter, hits, groundCheckDistance, true);
            Debug.DrawRay(footCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(rightSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(leftSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(headSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            return (hitCount > 0, wasGrounded, hits);
        }

        private void UpdateGrounded() {
            // if we are within a margin of starting a jump, we are still grounded
            // but we don't want to reset vertical momentum.
            if (Time.frameCount < jumpStartTime + groundCheckJumpMargin) {
                return;
            }
            (bool isGrounded, bool wasGrounded, List<RaycastHit2D> hits) = CheckGrounded();
            this.isGrounded = isGrounded;
            if (isGrounded) {
                hits.Sort((a, b) => a.distance.CompareTo(b.distance));
                groundedOnObject = hits[0].collider.gameObject;
                if (groundedOnObject.GetComponent<Rigidbody2D>() == null) {
                    Debug.LogWarning("Grounded on object without rigidbody2d: " + groundedOnObject.name);
                    groundedOnObject = null;
                } else {
                    if (groundedOnObject.GetComponent<Environment.IBouncable>() != null) {
                        if (groundedOnObject.GetComponent<Environment.IBouncable>().Bounce(this)) {
                            return;
                        }
                    }
                }
                jumpGrace = Time.frameCount + jumpGraceWindow;
                currentVerticalForce = rb.velocity.y;
                if (isJumping) {
                    Debug.Log("Player landed from jumping after " + (Time.frameCount - jumpStartTime) + " frames");
                    JumpStomp();
                    isJumping = false;
                    halfJump = false;
                    justLanded = true;
                    currentWalkingSpeed = currentWalkingSpeed * landingHorizontalDrag;
                } else if (!wasGrounded) {
                    Debug.Log("Player landed from falling after " + (Time.frameCount - frameLeftGround) + " frames");
                    if (Time.frameCount - frameLeftGround > 3) JumpStomp();
                    isJumping = false;
                    halfJump = false;
                    justLanded = true;
                }
            } else {
                if (Time.frameCount > jumpGrace) {
                    groundedOnObject = null;
                }
                if (wasGrounded) {
                    Debug.Log("Player left ground at " + Time.frameCount);
                    frameLeftGround = Time.frameCount;
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
                        isSlipping = true;
                        isSlippingLeft = isFacingRight;
                    }
                }
            }
        }

        private bool CheckHeadBump() {
            RaycastHit2D hit = Physics2D.Raycast(headCollider.position, Vector3.up, ceilingCheckDistance, LayerMask.GetMask("Ground"));
            Debug.DrawRay(footCollider.position, transform.TransformDirection(new Vector3(0, ceilingCheckDistance, 0)), Color.yellow, 0.1f);
            if (hit.collider != null) {
                if (hit.collider.gameObject.GetComponent<PlatformEffector2D>() != null) {
                    PlatformEffector2D platform = hit.collider.gameObject.GetComponent<PlatformEffector2D>();
                    if (platform.useOneWay) {
                        return false;
                    }
                }
                Debug.Log("Ouch! My fucking head!");
                return true;
            }
            return false;
        }

        bool CheckWallBump(float castDirection) {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Ground"));
            float numHits = bodyCollider.gameObject.GetComponent<Collider2D>().Cast(Vector2.right * castDirection, filter, hits, wallCheckDistance, true);
            if (numHits > 0) {
                Debug.Log("Walking into a wall");
                return true;
            }
            return false;
        }


        bool CheckWallBump() {
            float castDirection = (currentWalkingSpeed*lastHorizontalInput > 0 ? 1 : -1);
           return CheckWallBump(castDirection);
        }

        // end of movement functions
        // momentum 

        public void AddMomentum(Vector2 momentum) {
            
        }


        void UpdateMomentum() {
           
        }


        // activatables 

        void UseActivatable(IActivatables activatable, GameObject obj) {
            Debug.Log("Using activatable "+ obj.name);
            activatable.Activate(this);
            activatableInUse = activatable;
            AdjustFacing(obj.transform.position.x - transform.position.x);
            if (activatable.usageAnimation != "") {
                animator.SetBool(activatable.usageAnimation, true);
            }
            if (activatable.usageSound != null && activatable.usageSound != "") {
                game.sound.Play(activatable.usageSound);
            }
            ChangeState(activatable.playerState);
        }

        bool CheckForActivatables() {
            Collider2D[] colliders = new Collider2D[10];
            playerLocationTrigger.GetContacts(colliders);
            foreach (Collider2D coll in colliders) {
                if (coll != null && coll.gameObject != null && coll.CompareTag("Activatable")) {
                    Debug.Log("Can activate " + coll.name);
                    IActivatables activatable = game.GetActivatableComponent(coll.gameObject);
                    if (activatable.ActivationCondition(this) ) {
                        UseActivatable(activatable, coll.gameObject);
                        return true;
                    }
                }
            }
            return false;
        }

        public void DetatchFromActivatable() {
            Debug.Log("stopped using " + activatableInUse.ToString());
            if (activatableInUse.usageAnimation != "") {
                animator.SetBool(activatableInUse.usageAnimation, false);
            }
            if (activatableInUse.usageSound != "" && game.sound.IsPlaying(activatableInUse.usageSound)) {
                game.sound.Stop(activatableInUse.usageSound);
            }
            activatableInUse = null;
            ChangeState(PlayerStateName.ready);
        }
        

        public bool ActivateInput(bool keyDown) {
            if (keyDown && activatableInUse != null && activatableInUse.canManuallyDeactivate) {
                activatableInUse.Deactivate(this);
                DetatchFromActivatable();
                return true;
            } else if (keyDown) {
                return CheckForActivatables();
            }
            return false;
        }


        // Health

        public void PlayerDies(bool isFallToDeath) {
            if (isFallToDeath) {
                game.sound.Play("DeathFall");
            } else {
                game.sound.Play("Death");
            } 
            animator.SetTrigger("Dead");
            GameModel.Instance.TriggerGameOver();
        }



        public bool IsPlayerInvulnerable() {
            return Time.frameCount < invincibilityFrames + hitOnframe;
        }

        public void PlayerTakesDamage() {
            if (IsPlayerInvulnerable()) {
                return;
            }
            hitOnframe = Time.frameCount;
            ChangeState(PlayerStateName.hitstun);
            health -= 1;
            if (health <= 0) {
                PlayerDies(false);
                return;
            }  
            animator.SetTrigger("Hit");
            SoundManager.Instance.Play("Hit"); 
            if (activatableInUse != null) {
                activatableInUse.Deactivate(this);
                DetatchFromActivatable();
            }
            if (health == 1) {
                game.helpPrompts.AddPrompt(criticalHealthPrompt,
                () => { return health > 1; });
            }
            
        }

        public void PlayerHeals() {
            health = maxHealth;
        }

        // Update functions
         
        void MomentumUpdate() {
            List<Collider2D> momentumImpartingColliders = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("MomentumHazards"));
            filter.useTriggers = true;
            int numInContact =  Physics2D.OverlapCollider(GetComponent<Collider2D>(), filter, momentumImpartingColliders);
            foreach (Collider2D coll in momentumImpartingColliders) {
                if (coll != null && coll.gameObject != null) {
                    Environment.MomentumImpartingEffect momentumImpartingEffect = coll.gameObject.GetComponent<Environment.MomentumImpartingEffect>();
                    if (Mathf.Abs(momentumImpartingEffect.horizontalMomentum) > Mathf.Abs(currentHazardMomentum)) {
                        currentHazardMomentum = momentumImpartingEffect.horizontalMomentum;
                        lastAppliedHazardMomentum = currentHazardMomentum;
                        lastMomentumAppliedTime = Time.time;
                    }
                }
            }
            float decayStartTime = momentumDecayStartTime;
            if (isGrounded) {
                decayStartTime = decayStartTime/groundedMomentumDecayDivisor;
            }
            if (Time.time - lastMomentumAppliedTime > momentumDecayStartTime) {
                
                float decay = ((Time.time - lastMomentumAppliedTime) - momentumDecayStartTime) / (momentumDecayTime / (isGrounded ? groundedMomentumDecayDivisor : 1));
                if (decay > 1) {
                    decay = 1;
                }
                currentHazardMomentum = lastAppliedHazardMomentum * (1 - decay);
            }
        }

        void SoundUpdate() {
            // play walking sound when moving in the ready state on the ground
            if (!game.sound.IsPlaying("Walk") && currentPlayerStateName == PlayerStateName.ready && currentWalkingSpeed != 0 && isGrounded) {
                game.sound.Play("Walk"); 
            } else if (game.sound.IsPlaying("Walk") && (currentPlayerStateName != PlayerStateName.ready || currentWalkingSpeed == 0 || !isGrounded)) {
                game.sound.Stop("Walk");
            }

            if (health == 1 && !game.sound.IsPlaying("LowHP")) {
                game.sound.Play("LowHP"); 
            }

        }

        void MovementUpdate() {
            if (currentPlayerState.stealVelocityControl) {
                return;
            }
            UpdateGrounded();
            MomentumUpdate();
            bool headBump = CheckHeadBump();
            updateJump(headBump);
            float totalHorizontalVelocity = (currentWalkingSpeed*lastHorizontalInput) + currentHazardMomentum;
            float totalVerticalVelocity = currentVerticalForce;

            if (groundedOnObject != null) {
                totalHorizontalVelocity += groundedOnObject.GetComponent<Rigidbody2D>().velocity.x;
                totalVerticalVelocity += groundedOnObject.GetComponent<Rigidbody2D>().velocity.y;
                if (groundedOnObject.GetComponent<Environment.RotatingPlatformMovementHelper>() != null) {
                    Vector3 positionalChange = groundedOnObject.GetComponent<Environment.RotatingPlatformMovementHelper>().lastPositionChange;
                    totalHorizontalVelocity = totalHorizontalVelocity + (positionalChange.x / Time.deltaTime);
                    totalVerticalVelocity = totalVerticalVelocity + (positionalChange.y /Time.deltaTime);
                }
            }
            if (CheckWallBump(Mathf.Sign(totalHorizontalVelocity))) {
                totalHorizontalVelocity = 0;
            }
            if (headBump) {
                totalVerticalVelocity = Mathf.Min(totalVerticalVelocity, -slipSpeedVertical);
            }

            Vector2 movement = new Vector2(totalHorizontalVelocity, totalVerticalVelocity);
           
            rb.velocity = movement;
        }

        void animatorUpdate() {
            if (currentPlayerStateName == PlayerStateName.ready) {
                animator.SetFloat("Speed", Mathf.Abs(currentWalkingSpeed));
            } else {
                animator.SetFloat("Speed", 0);
                animator.SetBool("HoldingObject", false);
            }
            animator.SetBool("Grounded", isGrounded);
        }


        // this method is unused; replaced by UpdateState; 
        /*
        void InputUpdate(float deltaTime) {
            // get input
            bool playerStateWasAiming = currentPlayerStateName == PlayerStateName.aiming;
            bool activateKeyDown = Input.GetKeyDown(KeyCode.F);
            ActivateInput(activateKeyDown);
            if (playerStateWasAiming) {
                return;
            }
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
            ItemInput(pickItemDown);
            ItemUsageInput(useItemDown);

            // Camera Toggles
            if (Input.GetKeyDown(KeyCode.C)) {
                cameraControls?.ToggleShipView();
            }
        }
        */

        void Update() {
            if (game.isPaused) {
                lastJumpStompFrame += 1;
                jumpGrace += 1;
                jumpStartTime += 1;
                hitOnframe += 1;
                return;
            }
            //InputUpdate(deltaTime);
            MovementUpdate();
            animatorUpdate();
            SoundUpdate();
            currentPlayerState.UpdateState();
        }

        //input functions


    }


    public class CthulkInput {

        public static bool JumpKeyDown() {
            return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W);
        }

        public static bool JumpKeyHeld() {
            return Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
        }

        public static bool CrouchHeld() {
            return Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.LeftControl);
        }

        public static bool ActivateKeyDown() {
            return Input.GetKeyDown(KeyCode.F);
        }

        public static bool PickItemDown() {
            return Input.GetKeyDown(KeyCode.E);
        }

        public static bool UseItemDown() {
            return Input.GetKeyDown(KeyCode.F);
        }

        public static float HorizontalInput() {
            return Input.GetAxisRaw("Horizontal");
        }

        public static bool CameraToggleDown() {
            return Input.GetKeyDown(KeyCode.C);
        }
    }
}