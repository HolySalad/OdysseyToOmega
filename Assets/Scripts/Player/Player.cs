using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;
using SpaceBoat.Ship;

namespace SpaceBoat {
    public class Player : MonoBehaviour
    {
  
        [Header("General Player Settings")]
        [SerializeField] private GameObject playerCamera;
        [SerializeField] private int hitStunFrames = 24;
        [SerializeField] private int invincibilityFrames = 50;
        [SerializeField] public int maxHealth = 3;

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
        [SerializeField] private int jumpGraceWindow = 2;
        [SerializeField] private int halfJumpFrameWindow = 6;
        [SerializeField] private int jumpSquatFrames = 3;
        [SerializeField] private float halfJumpDecayMultiplier = 1.7f;
        [SerializeField] private float jumpPower = 22f;
        [SerializeField] private float jumpDecay = 28f;
        [SerializeField] private int jumpDecayDoublingFrames = 4;
        [SerializeField] private float gravityAcceleration = 30f;
        [SerializeField] private float slipSpeedVertical = 10f;
        [SerializeField] private float gravityTerminalVelocity = 45f;
        [SerializeField] private float jumpHorizontalMultiplier = 1.2f;
        [SerializeField] private float jumpHorizontalSpeedWindow = 0.5f;
        [SerializeField] private float landingHorizontalDrag = 0.7f;


        [Header("Walk Movement Settings")]
        [SerializeField] private float maxWalkSpeed = 6f;
        [SerializeField] private float maxHoriontalVelocity = 10f;

        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float deceleration = 7f;
        [SerializeField] private float accelerationStartMult = 5f;
        [SerializeField] private float accelerationMidMult = 2f;
        [SerializeField] private float accelerationStartRange = 1f;
        [SerializeField] private float accelerationMidRange = 3f;
        [SerializeField] private float turningSpeedMult = 0.7f;
        
        [Header("Momentum Settings")]
        [SerializeField] private float momentumDecayHorizontal = 10f;
        [SerializeField] private float momentumDecayVertical = 15f;
        [SerializeField] private int momentumAccelerationTime = 12; //frames to reach max momentum

        // References
        private GameModel game;
        private Animator animator;
        private Rigidbody2D rb;
        private Collider2D playerLocationTrigger;
        private Transform itemPlace;
        private Transform originOverride;

        // player states
        //possible states: ready, working, hitstun
        public enum PlayerState {ready, working, hitstun, aiming};
        public PlayerState playerState = PlayerState.ready;

        // internal gameplay vars
        public int health {get; private set;}
        private int hitOnframe;
        private bool needsHitSound = false;

        //internal movement vars
        private bool isGrounded = false;
        private bool groundedOnHazard = false;
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

        //item vars
        public IHeldItems itemInHand {get; private set;}
        private ItemTypes heldItemType;
        private int itemUsageBeganFrame = 0;
        private string itemUsageSound;
        private bool canPickItems;
        private GameObject itemUsageTarget;

        //activatables

        public IActivatables activatableInUse {get; private set;}

        public UI.CameraControls cameraControls;

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

            if (playerCamera == null) {
                playerCamera = GameObject.Find("MainCamera");
            }
            cameraControls = playerCamera?.GetComponent<UI.CameraControls>();
        }

        // walk script
        // handles horizontal movement and sprite orientation

        void AdjustFacing(float horizontalInput) {
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
            if (playerState != PlayerState.ready) {return false;}
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

        // item functions



        void PickupItems(GameObject itemObject) {
            SpriteRenderer render = itemObject.GetComponent<SpriteRenderer>();
            itemPlace.GetComponent<SpriteRenderer>().sprite = render.sprite;    //make the item a child so it follows the player
            itemPlace.GetComponent<SpriteRenderer>().transform.localScale = new Vector3(1, 1, 1);

            heldItemType = game.GetItemType(itemObject);
            Destroy(itemObject);
            itemInHand = game.CreateItemComponent(this.gameObject, heldItemType);
            itemInHand.currentlyHeld = true;
            game.sound.Play("PickItemUp");

            Debug.Log("Picked up " + heldItemType);
            Debug.Log("Item in hand: " + itemInHand);
        }


        private void CheckForItems(){
            Collider2D[] colliders = new Collider2D[10];
            int contacts = playerLocationTrigger.GetContacts(colliders);
            if (contacts == 0) {return;}
            foreach(Collider2D collider in colliders){
                if (collider == null) {continue;}
                Debug.Log(collider.name);
                if(collider.gameObject.CompareTag("Collectable")){
                    PickupItems(collider.gameObject);
                    return;
                }
            }
        }

        void DropItems(bool destroy) {
            itemPlace.GetComponent<SpriteRenderer>().sprite = null;

            if (!destroy) {
                GameObject droppedItem = Instantiate(game.PrefabForItemType(heldItemType), originOverride.position, Quaternion.identity);
                game.CreateItemComponent(droppedItem, heldItemType);
            }
            heldItemType = ItemTypes.None;
            itemInHand = null;
            game.sound.Play("DropItem");
        }

        void DropItems() {
            DropItems(false);
        }

        void ItemInput(bool keyDown) {
            if (keyDown && playerState == PlayerState.ready) {
                if (itemInHand != null) {
                    DropItems();
                } else if (itemInHand == null) {
                    CheckForItems();
                }
            }
        }

        // Item usage functions

        void useItem(GameObject target) {
            playerState = PlayerState.working;
            itemUsageBeganFrame = Time.frameCount;
            itemUsageTarget = target;
            itemUsageSound = itemInHand.itemUsageSound;
            if (itemInHand.usageAnimation != "") {
                animator.SetBool(itemInHand.usageAnimation, true);
            }
        }

        void updateItemUsage(int frameCount) {
            if (playerState == PlayerState.working && frameCount > itemUsageBeganFrame + itemInHand.usageFrames) {
                if (itemInHand.usageAnimation != "") {
                    animator.SetBool(itemInHand.usageAnimation, false);
                }
                itemInHand.ItemUsed(this, itemUsageTarget);
                if (itemInHand.isConsumed) {
                    DropItems(true);
                }
                itemUsageTarget = null;
                playerState = PlayerState.ready;
            } else if (playerState != PlayerState.working) {
                if (itemInHand != null) { 
                    if (itemInHand.usageAnimation != "") {
                        animator.SetBool(itemInHand.usageAnimation, false);
                    }
                    if (itemInHand.itemUsageSound != "" && game.sound.IsPlaying(itemInHand.itemUsageSound)) {
                        game.sound.Stop(itemInHand.itemUsageSound);
                    }
                }
                itemUsageBeganFrame = 0;
                itemUsageTarget = null;
            }
        }

        (bool, GameObject) canUseItem(IHeldItems item) {
            Collider2D[] colliders = new Collider2D[10];
            playerLocationTrigger.GetContacts(colliders);
            foreach (Collider2D coll in colliders) {
                if (coll != null && coll.CompareTag(item.itemUsageValidTrigger)) {
                    Debug.Log("Can use held item on " + coll.name);
                    return (item.itemUsageCondition(this, coll.gameObject), coll.gameObject);
                }
            }
            return (false, null);
        }



        void itemUsageInput(bool keyDown) {
            if (keyDown && playerState == PlayerState.ready) {
                if (itemInHand != null) {
                    (bool canUse, GameObject target) = canUseItem(itemInHand);
                    if (canUse) {
                        useItem(target);
                    }
                }
            } else if (keyDown && playerState == PlayerState.working) {
                playerState = PlayerState.ready;
            }
        }

        // activatables 

        void UseActivatable(IActivatables activatable, GameObject obj) {
            Debug.Log("Using activatable "+ obj.name);
            activatable.Activate(this);
            activatableInUse = activatable;
            playerState = activatable.playerState;
            AdjustFacing(obj.transform.position.x - transform.position.x);
            if (activatable.usageAnimation != "") {
                animator.SetBool(activatable.usageAnimation, true);
            }

        }

        void CheckForActivatables() {
            Collider2D[] colliders = new Collider2D[10];
            playerLocationTrigger.GetContacts(colliders);
            foreach (Collider2D coll in colliders) {
                if (coll != null && coll.gameObject != null && coll.CompareTag("Activatable")) {
                    Debug.Log("Can activate " + coll.name);
                    IActivatables activatable = game.GetActivatableComponent(coll.gameObject);
                    if (activatable.ActivationCondition(this) ) {
                        UseActivatable(activatable, coll.gameObject);
                    }
                }
            }
        }

        public void DetatchFromActivatable() {
            Debug.Log("stopped using " + activatableInUse.ToString());
            if (activatableInUse.usageAnimation != "") {
                animator.SetBool(activatableInUse.usageAnimation, false);
            }
            playerState = PlayerState.ready;
            activatableInUse = null;
        }
        

        void ActivateInput(bool keyDown) {
            if (keyDown && playerState == PlayerState.aiming && activatableInUse != null && activatableInUse.canManuallyDeactivate) {
                activatableInUse.Deactivate(this);
                DetatchFromActivatable();
            } else if (keyDown && playerState == PlayerState.ready) {
                CheckForActivatables();
            }
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
            needsHitSound = true;
            hitOnframe = Time.frameCount;
            playerState = PlayerState.hitstun;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), true);
            health -= 1;
            if (health <= 0) {
                PlayerDies(false);
            } else {
                animator.SetTrigger("Hit");
                SoundManager.Instance.Play("Hit"); 
                if (activatableInUse != null) {
                    activatableInUse.Deactivate(this);
                    DetatchFromActivatable();
                }
                if (health == 1) {
                    game.helpPrompts.DisplayPromptWithDeactivationCondition(game.helpPrompts.criticalPlayerPrompt, () => { return health > 1; });
                }
            }
        }

        public void PlayerHeals() {
            health = maxHealth;
        }

        // Update functions

        void HitStunUpdate(int frameCount) {
            if (playerState == PlayerState.hitstun && frameCount > hitStunFrames + hitOnframe) {
                playerState = PlayerState.ready;
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), false);
            }
        }

        void SoundUpdate() {
            // play walking sound when moving in the ready state on the ground
            if (!game.sound.IsPlaying("Walk") && playerState == PlayerState.ready && speed != 0 && isGrounded) {
                game.sound.Play("Walk"); 
            } else if (game.sound.IsPlaying("Walk") && (playerState != PlayerState.ready || speed == 0 || !isGrounded)) {
                game.sound.Stop("Walk");
            }

            // play the working sound when working;
            if (playerState == PlayerState.working && itemUsageSound != "" && !game.sound.IsPlaying(itemUsageSound)) {
                game.sound.Play(itemUsageSound);
            }
            
            if (needsHitSound) {
                game.sound.Play("Hit");
                needsHitSound = false;
            }

            if (health == 1 && !game.sound.IsPlaying("LowHP")) {
                game.sound.Play("LowHP"); 
            }

        }

        void MovementUpdate(float deltaTime) {
            CheckGrounded();
            if (isJumping) {
                CheckHeadBump();
            }
            updateJump(deltaTime);
            float horizontal = lastHorizontal;

            Vector2 movement = new Vector2(Mathf.Min((speed*lastHorizontal)+horizontalMomentum, maxHoriontalVelocity), currentVerticalForce + verticalMomentum);
            if (playerState == PlayerState.working || playerState == PlayerState.aiming) {
                movement = new Vector2(0, Mathf.Min(currentVerticalForce, 0));
            } else if (playerState == PlayerState.hitstun) {
                //TODO add hitstun knockback
                movement = new Vector2(horizontalMomentum, Mathf.Min(currentVerticalForce, 0) + verticalMomentum);
            }
            rb.velocity = movement;
        }

        void animatorUpdate() {
            if (playerState == PlayerState.ready) {
                animator.SetFloat("Speed", Mathf.Abs(speed));
                animator.SetBool("HoldingObject", (itemInHand != null));
            } else {
                animator.SetFloat("Speed", 0);
                animator.SetBool("HoldingObject", false);
            }
            animator.SetBool("Grounded", isGrounded);
        }

        void InputUpdate(float deltaTime) {
            // get input
            bool playerStateWasAiming = playerState == PlayerState.aiming;
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

            WalkInput(horizontal, deltaTime);
            JumpInput(jumpKeyHeld, jumpKeyDown);
            ItemInput(pickItemDown);
            itemUsageInput(useItemDown);

            // Camera Toggles
            if (Input.GetKeyDown(KeyCode.C)) {
                cameraControls?.ToggleShipView();
            }
        }

        void Update() {
            float deltaTime = Time.deltaTime;
            int frameCount = Time.frameCount;
            HitStunUpdate(frameCount);
            InputUpdate(deltaTime);
            UpdateMomentum(deltaTime);
            MovementUpdate(deltaTime);
            updateItemUsage(frameCount);
            animatorUpdate();
            SoundUpdate();
        }

        //input functions


    }
}