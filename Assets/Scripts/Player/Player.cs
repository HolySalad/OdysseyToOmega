using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship.Activatables;
using SpaceBoat.PlayerSubclasses.PlayerStates;
using SpaceBoat.PlayerSubclasses.Equipment;
using SpaceBoat.Rewards;

using SpaceBoat.UI;

namespace SpaceBoat {
    public enum PlayerStateName {ready, working, hitstun, turret, weaponEquipment, ladder, dash, ball, staticEquipment, captured, uiPauseState, nullState};
    public class Player : MonoBehaviour
    {
        [Header("General Player Settings")]
        [SerializeField] private int invincibilityFrames = 50;
        [SerializeField] public int maxHealth = 3;
        [SerializeField] public EquipmentType startingEquipment = EquipmentType.None;

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
        [SerializeField] public float gravityAcceleration = 30f;
        [SerializeField] private float fastfallMultiplier = 2f;
        [SerializeField] private float slipSpeedVertical = 10f;
        [SerializeField] public float gravityTerminalVelocity = 45f;
        [SerializeField] private float jumpHorizontalMultiplier = 1.2f;
        [SerializeField] private float jumpHorizontalSpeedWindow = 0.5f;
        [SerializeField] private float landingHorizontalDrag = 0.7f;
        [SerializeField] private int landingJumpKeyHoldBuffer = 4;
        [SerializeField] private int jumpBufferFrames = 4;


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

    /*
        [Header("Attack Settings")]
        [SerializeField] private GameObject weapon;
        [SerializeField] private bool attackEnabled = true;
        [SerializeField] private float attackHitboxLingerTime = 0.1f;
        [SerializeField] private float attackSpeed = 0.15f;
        [SerializeField] private float attackRetractionSpeed = 0.15f;
        [SerializeField] private float idleHarpoonRotation = 62f;
        [SerializeField] private float horizontalAttackHarpoonRotation = 11f;
        [SerializeField] private Vector2 horizontalHarpoonAttackOffset = new Vector2(0.4f, -0.5f);
        [SerializeField] private float upwardsVerticalAttackHarpoonRotation = 85f;
        [SerializeField] private Vector2 upwardsVerticalHarpoonAttackOffset = new Vector2(-0.1f, 0.7f);
        [SerializeField] private float downwardsVerticalAttackHarpoonRotation = -85f;
        [SerializeField] private Vector2 downwardsVerticalHarpoonAttackOffset = new Vector2(-0.1f, -1.7f);
        */

        //[SerializeField] private float momentumDecayHorizontal = 10f;
        //[SerializeField] private float momentumDecayVertical = 15f;
        //[SerializeField] private int momentumAccelerationTime = 12; //frames to reach max momentum

        // References
        private GameModel game;
        public Animator animator;
        private Rigidbody2D rb;
        private Collider2D playerLocationTrigger;

        private Transform originOverride;

        // player states
        //possible states: ready, working, hitstun
        public PlayerStateName currentPlayerStateName = PlayerStateName.ready;
        private IPlayerState currentPlayerState;
        private Dictionary<PlayerStateName, IPlayerState> playerStates = new Dictionary<PlayerStateName, IPlayerState>();

        //player equipment
        private IPlayerEquipment currentEquipment;
        public EquipmentType currentEquipmentType = EquipmentType.None;
        private Dictionary<EquipmentType, IPlayerEquipment> equipmentScripts = new Dictionary<EquipmentType, IPlayerEquipment>();


        // internal gameplay vars
        public int health {get; private set;}
        private int hitOnframe;

        public int money {
            get {
                return game.saveGame.money;
            }
            private set {
                game.saveGame.money = value;
            }
        }

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
        private int frameJumpPressed = 0;
        private bool  jumpSquat = false;
        private bool isJumping = false;
        private bool fastFall = false;
        private bool halfJump = false;
        private int jumpStartFrame = 0;
        private float currentVerticalForce  = 0f;
        private bool hitApex = false;
        public (bool, bool, bool, bool) GetJumpStatus() {
            return (isJumping, fastFall, halfJump, hitApex);
        }

        private float currentJumpStompCooldown = 0;
        private int jumpStompCooldown = 18;

        private bool hasJumpPowerOverride = false;
        private float jumpPowerOverride = 0f;

        private bool isFacingRight = true;
        public float GetFacingDirection() {
            return isFacingRight ? 1f : -1f;
        }

        private float lastHorizontalInput;
        private float currentWalkingSpeed;
        private int landedAtFrame = 0;

        public bool isSlipping {get; private set;} = false;
        private bool isSlippingLeft = false;

        //imparted momentum
        
        private float currentHazardMomentum = 0f;
        private float lastAppliedHazardMomentum = 0f;
        private float lastMomentumAppliedTime = 0f;
        //private Dictionary<Environment.MomentumImpartingEffectKind, momentumEntry> momentumEntries = new Dictionary<Environment.MomentumImpartingEffectKind, momentumEntry>();


        //activatables

        public IActivatables activatableInUse {get; private set;}
        public GameObject activatableInRange {get; private set;}
        public float playerCameraXFocusOffset;

        // Callbacks
        public delegate void PlayerCallback(Player player);
        private List<PlayerCallback> onPlayerLandedCallbacks = new List<PlayerCallback>();
        public void AddOnPlayerLandedCallback(PlayerCallback callback) {
            onPlayerLandedCallbacks.Add(callback);
        }
        private void CallOnPlayerLandedCallbacks() {
            foreach (PlayerCallback callback in onPlayerLandedCallbacks) {
                callback(this);
            }
        }
        private List<PlayerCallback> onPlayerHeadbumpCallbacks = new List<PlayerCallback>();
        public void AddOnPlayerHeadbumpCallback(PlayerCallback callback) {
            onPlayerHeadbumpCallbacks.Add(callback);
        }
        private void CallOnPlayerHeadbumpCallbacks() {
            foreach (PlayerCallback callback in onPlayerHeadbumpCallbacks) {
                callback(this);
            }
        }

        void Awake() {
            //fill references
            game = FindObjectOfType<GameModel>();
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody2D>();
            originOverride = transform.Find("OriginOverride").transform;
            playerLocationTrigger = GetComponent<Collider2D>();

            //set default values
            health = maxHealth;


            //set up player states
            playerStates.Add(PlayerStateName.ready, GetComponent<ReadyState>() ?? gameObject.AddComponent<ReadyState>());
            playerStates.Add(PlayerStateName.hitstun, GetComponent<HitstunState>() ?? gameObject.AddComponent<HitstunState>());
            playerStates.Add(PlayerStateName.working, GetComponent<WorkingState>() ?? gameObject.AddComponent<WorkingState>());
            playerStates.Add(PlayerStateName.turret, GetComponent<TurretState>() ?? gameObject.AddComponent<TurretState>());
            playerStates.Add(PlayerStateName.ladder, GetComponent<LadderState>() ?? gameObject.AddComponent<LadderState>());
            playerStates.Add(PlayerStateName.dash, GetComponent<DashState>() ?? gameObject.AddComponent<DashState>());
            playerStates.Add(PlayerStateName.weaponEquipment, GetComponent<WeaponEquipmentState>() ?? gameObject.AddComponent<WeaponEquipmentState>());
            playerStates.Add(PlayerStateName.staticEquipment, GetComponent<StaticEquipmentState>() ?? gameObject.AddComponent<StaticEquipmentState>());
            playerStates.Add(PlayerStateName.uiPauseState, GetComponent<UIPauseState>() ?? gameObject.AddComponent<UIPauseState>());
            playerStates.Add(PlayerStateName.captured, GetComponent<CapturedState>() ?? gameObject.AddComponent<CapturedState>());


            currentPlayerState = playerStates[currentPlayerStateName];

            //set up equipment
            equipmentScripts.Add(EquipmentType.None, GetComponent<NoneEquipment>());
            equipmentScripts.Add(EquipmentType.Dash, GetComponent<DashEquipment>());
            equipmentScripts.Add(EquipmentType.HealthPack, GetComponent<HealthPackEquipment>());
            equipmentScripts.Add(EquipmentType.HarpoonLauncher, GetComponent<HarpoonLauncherEquipment>());
            equipmentScripts.Add(EquipmentType.Shield, GetComponent<ShieldEquipment>());

            currentEquipment = equipmentScripts[currentEquipmentType];
            currentEquipment.Equip(this);
            if (startingEquipment != currentEquipmentType) {
                ChangeEquipment(startingEquipment);
            }

            string[] joysticknames = Input.GetJoystickNames();
            for (int i = 0; i < joysticknames.Length; i++) {
                Debug.Log(joysticknames[i]);
            }
        }
        
        public void ChangeState(PlayerStateName newStateName) {
            if (currentPlayerStateName != newStateName && newStateName != PlayerStateName.nullState) {
                PlayerStateName oldStateName = currentPlayerStateName;
                currentPlayerStateName = newStateName;
                currentPlayerState = playerStates[newStateName];
                playerStates[oldStateName].ExitState(newStateName);
                //if the exit state function redirected to another state
                if (currentPlayerStateName != newStateName) return;
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

        private bool isStepFirst = true;
        public void Footfall() {
            if (isStepFirst) {
                game.sound.Play("Footstep1", 0.6f);
            } else {
                game.sound.Play("Footstep2", 0.4f);
            }
            isStepFirst = !isStepFirst;
        }

        public void OverrideWalkSpeed(float speed) {
            currentWalkingSpeed = speed;
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
                jumpStartFrame = Time.frameCount + jumpSquatFrames;
                jumpSquat = true;
                animator.SetTrigger("Jump");
            }
        }

        private bool CanJump() {
            return Time.frameCount < jumpGrace || isGrounded;
        }

        void updateJump(bool headBumped) {
            // start the jump
            if (jumpSquat && Time.frameCount > jumpStartFrame ) {
                SoundManager sm = FindObjectOfType<SoundManager>();
                sm.Play("Jump", 0.5f); 
                Debug.Log("JumpSquat > Jump");
                jumpSquat = false;
                if (headBumped) {
                    landedAtFrame = Time.frameCount;
                    return;
                }
                isJumping = true;
                isGrounded = false;
                hitApex = false;
                currentVerticalForce = jumpPower;
                if (hasJumpPowerOverride) {
                    currentVerticalForce = jumpPowerOverride;
                    hasJumpPowerOverride = false;
                }
                if (currentWalkingSpeed > maxWalkSpeed * jumpHorizontalSpeedWindow) {
                    currentWalkingSpeed = currentWalkingSpeed * jumpHorizontalMultiplier;
                }
                return;
            }   

            if (headBumped) {
                currentVerticalForce = slipSpeedVertical*(1-landingHorizontalDrag);
            } else if (currentVerticalForce > 0) {
                float decay = 0;
                if ((halfJump && Time.frameCount > jumpStartFrame + halfJumpEarliestFrame) || (Time.frameCount > jumpStartFrame + jumpDecayStartFrame)) {
                    decay = jumpDecay;
                }
                currentVerticalForce = Mathf.Max(0, currentVerticalForce - decay * Time.deltaTime);
                if (isCrouched) {
                    fastFall = true;
                }
            } else if (!isGrounded) {
                if (!hitApex) {
                    Debug.Log("Hit Apex after " + (Time.frameCount - jumpStartFrame) + " frames");
                    hitApex = true;
                    //TODO jump animation > fall animation
                }
                if (isSlipping) {
                    currentVerticalForce = -slipSpeedVertical;
                } else {
                    float currentGravity = gravityAcceleration * Time.deltaTime;
                    if (fastFall) {
                        currentGravity = currentGravity * fastfallMultiplier;
                    }
                    currentVerticalForce = Mathf.Max(-gravityTerminalVelocity, currentVerticalForce - currentGravity);
                }
            }
        }

        public void JumpInput(bool keyHeld, bool keyDown) {
            if (keyDown) {
                frameJumpPressed = Time.frameCount;
            }
            bool jumpIsBuffered = Time.frameCount - frameJumpPressed < jumpBufferFrames;
            bool jumpPress = keyDown || jumpIsBuffered || (keyHeld && Time.frameCount - landedAtFrame > landingJumpKeyHoldBuffer);
            if (jumpPress && !isJumping) {
                StartJump();
            } else if (!keyHeld && isJumping && !halfJump) {
                Debug.Log("Half Jump");
                halfJump = true;
            }
        }

        public void ForceJump(bool lockOutReadyState = false, bool halfJump = false, bool skipJumpSquat = false, float jumpPowerMult = -1) {
            StartJump(true);
            if (skipJumpSquat) {
                jumpStartFrame = Time.frameCount;
            }
            if (halfJump) {
                this.halfJump = true;
            }
            if (jumpPowerMult > 0) {
                hasJumpPowerOverride = true;
                jumpPowerOverride = jumpPowerMult*jumpPower;
            }
            if (lockOutReadyState) {
                IPlayerState ready = playerStates[PlayerStateName.ready];
                if (ready is ReadyState) {
                    ((ReadyState)ready).JumpLockOut();
                }
            }
        }

        private void JumpStomp() {
            if (jumpStompCooldown <= 0) {
                currentJumpStompCooldown = jumpStompCooldown;
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

        public void OverrideVerticalForce(float force) {
            currentVerticalForce = force;
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
            if (Time.frameCount < jumpStartFrame + groundCheckJumpMargin) {
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
                fastFall = false;
                if (isJumping) {
                    Debug.Log("Player landed from jumping after " + (Time.frameCount - jumpStartFrame) + " frames");
                    JumpStomp();
                    isJumping = false;
                    halfJump = false;
                    landedAtFrame = Time.frameCount;
                    CallOnPlayerLandedCallbacks();
                    currentWalkingSpeed = currentWalkingSpeed * landingHorizontalDrag;
                } else if (!wasGrounded) {
                    Debug.Log("Player landed from falling after " + (Time.frameCount - frameLeftGround) + " frames");
                    if (Time.frameCount - frameLeftGround > 3) JumpStomp();
                    isJumping = false;
                    halfJump = false;
                    landedAtFrame = Time.frameCount;
                    CallOnPlayerLandedCallbacks();
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

        public bool CheckWallBump(float castDirection) {
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


        public bool CheckWallBump() {
            float castDirection = (currentWalkingSpeed*lastHorizontalInput > 0 ? 1 : -1);
           return CheckWallBump(castDirection);
        }

        // end of movement functions

        // player equipment

        public void CraftEquipment(EquipmentType type, int cost) {
            game.saveGame.equipmentBuilt[type] = true;
            PlayerSpendsMoney(cost);
        }

        public bool HasEquipment(EquipmentType type) {
            return game.saveGame.equipmentBuilt[type];
        }

        public void ChangeEquipment(EquipmentType type) {
            if (!equipmentScripts.ContainsKey(type)) {
                Debug.LogError("Player does not have equipment of type " + type + " registered in the player equipment dictionary");
                return;
            } 
            if (currentEquipment.isActive) {
                currentEquipment.CancelActivation(this);
            }
            currentEquipment.Unequip(this);

            currentEquipment = equipmentScripts[type];
            currentEquipmentType = type;
            currentEquipment.Equip(this);
        }

        bool CheckEquipmentActivation() {
            if (currentEquipment.ActivationCondition(this)) {
                currentEquipment.Activate(this);
                ChangeState(currentEquipment.usageState);
                return true;
            }
            return false;
        }

        public bool DeactivateEquipment() {
            currentEquipment.CancelActivation(this);
            if (currentEquipment.usageState != PlayerStateName.ready) {
                ChangeState(PlayerStateName.ready);
                return true;
            }
            return false;
        }


        public bool EquipmentUsageInput(bool keyDown, bool keyHeld) {
            switch (currentEquipment.activationBehaviour) {
                case EquipmentActivationBehaviour.Hold:
                    if (keyDown && !currentEquipment.isActive) {
                        return CheckEquipmentActivation();
                    } else if (!keyHeld && currentEquipment.isActive) {
                        return DeactivateEquipment();
                    }
                    break;
                case EquipmentActivationBehaviour.Toggle:
                    if (keyDown) {
                        if (currentEquipment.isActive) {
                            return DeactivateEquipment();
                        } else {
                            return CheckEquipmentActivation();
                        }
                    }
                    break;
                case EquipmentActivationBehaviour.Press:
                    if (keyDown && !currentEquipment.isActive) {
                        return CheckEquipmentActivation();
                    }
                    break;
                default:
                    if (EquipmentActivationBehaviour.None != currentEquipment.activationBehaviour) {
                        Debug.LogError("Equipment activation behaviour " + currentEquipment.activationBehaviour + " not implemented");
                    }
                    break;
            }
            return false;
        }

        //weapon 
        /*
        private enum AttackDirection {
            Horizontal, UpwardsVertical, DownwardsVertical
        }

        IEnumerator Attack(AttackDirection direction) {
            switch (direction) {
                case AttackDirection.Horizontal:
                    Vector2 weaponOffset = isFacingRight ? horizontalHarpoonAttackOffset : horizontalHarpoonAttackOffset * -1;
                    float attackTimer = 0;
                    Vector2 weaponOffsetChange = Time.deltaTime * weaponOffset / attackSpeed;
                    float weaponRotationChange = Time.deltaTime * horizontalAttackHarpoonRotation / attackSpeed;
                    if (!isFacingRight) {
                        weaponRotationChange = -weaponRotationChange;
                    }
                    while (attackTimer < attackSpeed) {
                        attackTimer += Time.deltaTime;
                        weapon.transform.position = transform.position + (Vector3)weaponOffsetChange;
                        weapon.transform.Rotate(0, 0, weapon.transform.rotation.eulerAngles.z + weaponRotationChange);
                        yield return null;
                    }
                    break;
            }
            weapon.GetComponent<Collider2D>().enabled = true;
            yield return new WaitForSeconds(attackHitboxLingerTime);
            weapon.GetComponent<Collider2D>().enabled = false;
            switch (direction) {
                case AttackDirection.Horizontal:
                    Vector2 weaponOffset = isFacingRight ? horizontalHarpoonAttackOffset : horizontalHarpoonAttackOffset * -1;
                    float attackTimer = 0;
                    Vector2 weaponOffsetChange = Time.deltaTime * weaponOffset / attackSpeed;
                    float weaponRotationChange = Time.deltaTime * horizontalAttackHarpoonRotation / attackSpeed;
                    while (attackTimer < attackSpeed) {
                        attackTimer += Time.deltaTime;
                        weapon.transform.position = transform.position - (Vector3)weaponOffsetChange;
                        weapon.transform.Rotate(0, 0, weapon.transform.rotation.eulerAngles.z - weaponRotationChange);
                        yield return null;
                    }
                    break;
            }

        }


        public void WeaponInput(bool keyDown) {
            if (keyDown) {
                StartCoroutine(Attack(AttackDirection.Horizontal));
            }
        */
        // activatables 

        bool UseActivatable(IActivatables activatable, GameObject obj) {
            if (activatableInRange == null) {
                return false;
            }
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
            return true;
        }

        bool CheckForActivatables() {
            Collider2D[] colliders = new Collider2D[10];
            playerLocationTrigger.GetContacts(colliders);
            foreach (Collider2D coll in colliders) {
                if (coll != null && coll.gameObject != null && coll.CompareTag("Activatable")) {
                    //Debug.Log("Can activate " + coll.name);
                    IActivatables activatable = game.GetActivatableComponent(coll.gameObject);
                    if (activatable.ActivationCondition(this) ) {
                        if (coll.gameObject != activatableInRange) {
                            activatableInRange = coll.gameObject;
                            if (activatable.HelpPrompt.promptLabel != "") game.controlsPrompts.AddPrompt(activatable.HelpPrompt);
                        }
                        return true;
                    }
                }
            }
            if (activatableInRange != null) {
                Debug.Log("Can no longer activate " + activatableInRange.ToString());
                IActivatables activatable = game.GetActivatableComponent(activatableInRange);
                if (activatable.HelpPrompt.promptLabel != "") game.controlsPrompts.RemovePrompt(activatable.HelpPrompt);
                activatableInRange = null;
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
            CheckForActivatables();
            if (keyDown && activatableInUse != null && activatableInUse.canManuallyDeactivate) {
                activatableInUse.Deactivate(this);
                DetatchFromActivatable();
                return true;
            } else if (keyDown) {
                return UseActivatable(game.GetActivatableComponent(activatableInRange), activatableInRange);
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
            if (currentEquipment is ShieldEquipment && currentEquipment.isActive) {
                ((ShieldEquipment)currentEquipment).TakeDamage(this);
                return;
            }
            hitOnframe = Time.frameCount;
            ChangeState(PlayerStateName.hitstun);
            health -= 1;
            if (health <= 0) {
                PlayerDies(false);
                return;
            }  
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

        //currency
        public void PlayerGainsMoney(int amount) {
            money += amount;
        }

        public void PlayerSpendsMoney(int amount) {
            money -= amount;
        }

        public bool PlayerHasMoney(int amount) {
            return money >= amount;
        }

        // Update functions
         
        public float MomentumUpdate() {
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
                currentHazardMomentum = Mathf.Max(0, lastAppliedHazardMomentum * (1 - decay));
            }
            return currentHazardMomentum;
        }

        void SoundUpdate() {
            if (health == 1 && !game.sound.IsPlaying("LowHP")) {
                game.sound.Play("LowHP"); 
            }

        }

        void MovementUpdate() {
            UpdateGrounded();
            MomentumUpdate();
            bool headBump = CheckHeadBump();
            if (headBump) {CallOnPlayerHeadbumpCallbacks();}
            if (currentPlayerState.stealVelocityControl) {
                return;
            }
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
                
                jumpGrace += 1;
                jumpStartFrame += 1;
                hitOnframe += 1;
                landedAtFrame += 1;
                return;
            }
            //update timers 
            currentJumpStompCooldown -= Time.deltaTime;


            //InputUpdate(deltaTime);
            currentEquipment.UpdateEquipment(this);
            MovementUpdate();
            animatorUpdate();
            SoundUpdate();
            currentPlayerState.UpdateState();
            //WeaponInput(CthulkInput.AttackKeyDown());
        }

    }




}