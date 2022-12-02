using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;
using SpaceBoat.Ship;

namespace SpaceBoat {
    public class Player : MonoBehaviour
    {
  
        [Header("General Player Settings")]
        [SerializeField] private int hitStunFrames = 24;
        [SerializeField] private int invincibilityFrames = 50;
        [SerializeField] public int maxHealth = 3;

        [Header("Collision Detection Settings")]
        [SerializeField] private float groundCheckDistance = 0.1f;
        [SerializeField] private float slipCheckDistance = 0.2f;
        [SerializeField] private int groundCheckJumpMargin = 24; //how many frames after jumping to check for ground
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private float ceilingCheckDistance = 0.1f;
        [SerializeField] private float slipSpeed = 4f;
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
        [SerializeField] private float gravityTerminalVelocity = 45f;


        [Header("Walk Movement Settings")]
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float deceleration = 7f;
        [SerializeField] private float accelerationStartMult = 5f;
        [SerializeField] private float accelerationMidMult = 2f;
        [SerializeField] private float accelerationStartRange = 1f;
        [SerializeField] private float accelerationMidRange = 3f;
        [SerializeField] private float turningSpeedMult = 0.7f;

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
        private int JumpGrace = 0;
        private int jumpGrace = 0;
        private bool  jumpSquat = false;
        private bool isJumping = false;
        private bool halfJump = false;
        private int jumpStartTime = 0;
        private float currentVerticalForce  = 0f;
        private bool hitApex = false;

        private bool isFacingRight = true;
        private bool isWalking;
        private float lastHorizontal;
        private float speed;

        private bool isSlipping = false;
        private bool isSlippingLeft = false;

        //item vars
        public IHeldItems itemInHand {get; private set;}
        private ItemTypes heldItemType;
        private int itemUsageBeganFrame = 0;
        private string itemUsageSound;
        private bool canPickItems;
        private GameObject itemUsageTarget;

        //activatables

        public IActivatables activatableInUse {get; private set;}


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
        }

        // walk script
        // handles horizontal movement and sprite orientation

        public void WalkInput(float horizontalInput, float deltaTime) {
            if (horizontalInput != 0) {lastHorizontal = horizontalInput;}
            if (horizontalInput == 0) {
                if (speed > 0) {
                    speed = Mathf.Max(speed - deceleration*deltaTime, 0);
                } else if (speed < 0) {
                    speed = Mathf.Min(speed + deceleration*deltaTime, 0);
                }
            } else {
                float accel = acceleration;
                if (Mathf.Abs(speed) < accelerationStartRange) {
                    accel *= accelerationStartMult;
                } else if (Mathf.Abs(speed) < accelerationMidRange) {
                    accel *= accelerationMidMult;
                }
                speed = Mathf.Min(speed + accel*deltaTime, maxSpeed);
            }
            if (isSlipping) {
                speed = (isSlippingLeft ? -1 : 1);
                isSlipping = false;
            } else if (speed != 0) {
                float castDirection = (speed*lastHorizontal > 0 ? 1 : -1);
                List<RaycastHit2D> hits = new List<RaycastHit2D>();
                ContactFilter2D filter = new ContactFilter2D();
                filter.layerMask = LayerMask.GetMask("Ground");
                float numHits = bodyCollider.gameObject.GetComponent<Collider2D>().Cast(Vector2.right * castDirection, filter, hits, wallCheckDistance, true);
                if (numHits > 0) {
                    Debug.Log("Walking into a wall");
                    speed = 0;
                }
            }
            if (horizontalInput > 0 && !isFacingRight || horizontalInput < 0 && isFacingRight) {
                isFacingRight = !isFacingRight;
                speed = -speed * turningSpeedMult;
                Vector3 existingColliderLocation = bodyCollider.transform.position;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                Vector3 colliderOffset =  existingColliderLocation - bodyCollider.transform.position;
                transform.position = new Vector3 (transform.position.x + colliderOffset.x, transform.position.y + colliderOffset.y, transform.position.z);
            }
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
            } else if (!isGrounded) {
                if (!hitApex) {
                    Debug.Log("Hit Apex after " + (Time.frameCount - jumpStartTime) + " frames");
                    hitApex = true;
                    //TODO jump animation > fall animation
                }
                currentVerticalForce = Mathf.Max(-gravityTerminalVelocity, currentVerticalForce - gravityAcceleration * deltaTime);
            }
        }

        private void JumpInput(bool keyDown) {
            if (keyDown && !isJumping) {
                StartJump();
            } else if (!keyDown && isJumping && Time.frameCount < jumpStartTime + halfJumpFrameWindow) {
                Debug.Log("Half Jump");
                halfJump = true;
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
            filter.layerMask = LayerMask.GetMask("Ground");
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            //RaycastHit2D hit = Physics2D.CircleCast(footCollider.position, footCollider.gameObject.GetComponent<Collider2D>().bounds.extents.x, new Vector3(0, -1, 0), groundCheckDistance, LayerMask.GetMask("Ground"));
            int hitCount = footCollider.gameObject.GetComponent<Collider2D>().Cast(new Vector3(0, -1, 0), filter, hits, groundCheckDistance, true);
            Debug.DrawRay(footCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(rightSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(leftSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            Debug.DrawRay(headSlipCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            isGrounded = hitCount > 0;
            if (isGrounded) {
                JumpGrace = Time.frameCount + jumpGraceWindow;
                if (isJumping) {
                    Debug.Log("Player landed from jumping after " + (Time.frameCount - jumpStartTime) + " frames");
                    game.sound.Play("JumpStomp"); 
                    isJumping = false;
                    halfJump = false;
                    currentVerticalForce = 0;
                } else if (!wasGrounded) {
                    Debug.Log("Player landed from falling after " + (Time.frameCount - jumpStartTime) + " frames");
                    game.sound.Play("JumpStomp");
                    isJumping = false;
                    halfJump = false;
                    currentVerticalForce = 0;
                }
            } else {
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

        /*echos the OnColliderEnter2D() function from the player's foot collider
        public void OnFColliderEnter(Collider2D coll, Collision2D other) {
            if (!isGrounded && (other.gameObject.layer == LayerMask.NameToLayer("Ground"))) {
                if (isJumping) {
                    game.sound.Play("JumpStomp"); 
                }
                isGrounded = true;
                isJumping = false;
                halfJump = false;
                currentVerticalForce = 0;
            }
        }

        //echos the OnColliderExit2D() function from the player's foot collider
        public void OnFColliderExit(Collider2D coll,Collision2D other) {
            if (other.gameObject.layer == LayerMask.NameToLayer("Ground") && !coll.IsTouchingLayers(LayerMask.GetMask("Ground"))) {
                //Debug.Log("Collision Exit from ground");
                jumpGrace = Time.frameCount + jumpGraceWindow;
                isGrounded = false;
            }
        }

        //does the same with the head collider for ceiling collisions
        public void OnHColliderEnter(Collider2D coll, Collision2D other) {
            if (!isGrounded) {
                currentVerticalForce = 0;
            }
        }

        */
        // end of movement functions

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
        }

        void updateItemUsage(int frameCount) {
            if (playerState == PlayerState.working && frameCount > itemUsageBeganFrame + itemInHand.usageFrames) {
                itemInHand.ItemUsed(this, itemUsageTarget);
                if (itemInHand.isConsumed) {
                    DropItems(true);
                }
                itemUsageTarget = null;
                playerState = PlayerState.ready;
            } else if (playerState != PlayerState.working) {
                itemUsageBeganFrame = 0;
                itemUsageTarget = null;
            }
        }

        (bool, GameObject) canUseItem(IHeldItems item) {
            Collider2D[] colliders = new Collider2D[10];
            playerLocationTrigger.GetContacts(colliders);
            foreach (Collider2D coll in colliders) {
                if (coll.CompareTag(item.itemUsageValidTrigger)) {
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
            }
        }

        // activatables 

        void CheckForActivatables() {
            Collider2D[] colliders = new Collider2D[10];
            playerLocationTrigger.GetContacts(colliders);
            foreach (Collider2D coll in colliders) {
                if (coll != null && coll.gameObject != null && coll.CompareTag("Activatable")) {
                    Debug.Log("Can activate " + coll.name);
                    IActivatables activatable = game.GetActivatableComponent(coll.gameObject);
                    if (activatable.ActivationCondition(this) ) {
                        activatable.Activate(this);
                        activatableInUse = activatable;
                        playerState = activatable.playerState;
                    }
                }
            }
        }

        public void DeactivateFromActivatable() {
            activatableInUse = null;
            playerState = PlayerState.ready;
        }
        

        void ActivateInput(bool keyDown) {
            if (keyDown && playerState == PlayerState.aiming && activatableInUse != null && activatableInUse.canManuallyDeactivate) {
                activatableInUse.Deactivate(this);
                activatableInUse = null;
                playerState = PlayerState.ready;
            }
            if (keyDown && playerState == PlayerState.ready) {
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
            animator.SetTrigger("Death");
            StartCoroutine(GameModel.Instance.GameOver());
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
            health -= 1;
            if (health <= 0) {
                PlayerDies(false);
            } else {
                animator.SetTrigger("Hit");
                SoundManager.Instance.Play("Hit"); 
            }
        }

        public void PlayerHeals() {
            health = maxHealth;
        }

        // Update functions

        void HitStunUpdate(int frameCount) {
            if (playerState == PlayerState.hitstun && frameCount > hitStunFrames + hitOnframe) {
                playerState = PlayerState.ready;
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

            Vector2 movement = new Vector2(speed*lastHorizontal, currentVerticalForce);
            if (playerState == PlayerState.working || playerState == PlayerState.aiming) {
                movement = new Vector2(0, Mathf.Min(currentVerticalForce, 0));
            } else if (playerState == PlayerState.hitstun) {
                //TODO add hitstun knockback
                movement = new Vector2(0, Mathf.Min(currentVerticalForce, 0));
            }
            rb.velocity = movement;
        }

        void animatorUpdate() {
            animator.SetFloat("Speed", Mathf.Abs(speed));
            animator.SetBool("Grounded", isGrounded);
            animator.SetBool("HoldingObject", (itemInHand != null));
        }

        void InputUpdate(float deltaTime) {
            // get input
            bool activateKeyDown = Input.GetKeyDown(KeyCode.F);
            ActivateInput(activateKeyDown);
            if (playerState ==  PlayerState.aiming) {
                return;
            }
            bool jumpKeyDown = Input.GetKey(KeyCode.Space);
            float horizontal = Input.GetAxisRaw("Horizontal");

            //Item pick up
            bool pickItemDown = Input.GetKeyDown(KeyCode.E);

            //Item Usage
            bool useItemDown = Input.GetKeyDown(KeyCode.Q); 

            WalkInput(horizontal, deltaTime);
            JumpInput(jumpKeyDown);
            ItemInput(pickItemDown);
            itemUsageInput(useItemDown);
        }

        void Update() {
            float deltaTime = Time.deltaTime;
            int frameCount = Time.frameCount;
            HitStunUpdate(frameCount);
            InputUpdate(deltaTime);
            MovementUpdate(deltaTime);
            updateItemUsage(frameCount);
            animatorUpdate();
            SoundUpdate();
        }

        //input functions


    }
}