using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;
using SpaceBoat.Ship;
using SpaceBoat.PlayerStates;

namespace SpaceBoat {
    public enum PlayerStateName {ready, working, hitstun, aiming, empty};

    //the main script for the player.
    // specifics of movements, and the controls, are handled by State Machine subscripts.
    // this script handles health, implements the state machine, and contains common functionality used by multiple states.
    
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
        [SerializeField] private float wallCheckDistance = 0.5f;
        [SerializeField] private float ceilingCheckDistance = 0.1f;
        [SerializeField] public Transform footCollider;
        [SerializeField] public Transform headCollider;
        [SerializeField] public Transform bodyCollider;
        [SerializeField] public Transform leftSlipCollider;
        [SerializeField] public Transform rightSlipCollider;
        [SerializeField] public Transform headSlipCollider;

        // References
        private GameModel game;
        private Animator animator;
        private Rigidbody2D rb;
        private Collider2D playerLocationTrigger;
        private Transform itemPlace;
        private Transform originOverride;

        // player states
        //possible states: ready, working, hitstun
        public PlayerStateName playerState = PlayerStateName.ready;

        private Dictionary<PlayerStateName, IPlayerState> playerStates = new Dictionary<PlayerStateName, IPlayerState>();

        // gameplay vars
        public int health {get; private set;}
        private int hitOnframe;
        private bool needsHitSound = false;

        // movement vars
        public bool isFacingRight {get; private set;} = true;
        public bool isGrounded  {get; private set;} = false;
        public bool groundedOnHazard {get; private set;} = false;
      
        public List<EntityMomentum> playerMomentum = new List<EntityMomentum>();

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

            if (GetComponent<ReadyState>() == null) {
                gameObject.AddComponent<ReadyState>();
                Debug.LogWarning("ReadyState was not found on the player - creating a component with default values. ");
            }
            if (GetComponent<WorkingState>() == null) {
                gameObject.AddComponent<WorkingState>();
                Debug.LogWarning("WorkingState was not found on the player - creating a component with default values. ");
            }
            if (GetComponent<HitstunState>() == null) {
                gameObject.AddComponent<HitstunState>();
                Debug.LogWarning("HitstunState was not found on the player - creating a component with default values. ");
            }
            if (GetComponent<AimingState>() == null) {
                gameObject.AddComponent<AimingState>();
                Debug.LogWarning("AimingState was not found on the player - creating a component with default values. ");
            }

            // set up state machine
            playerStates.Add(PlayerStateName.ready, GetComponent<ReadyState>());
            playerStates.Add(PlayerStateName.working, GetComponent<WorkingState>());
            playerStates.Add(PlayerStateName.hitstun, GetComponent<HitstunState>());
            playerStates.Add(PlayerStateName.aiming, GetComponent<AimingState>());
        }
        
        // state machine functions
        public void EnterState(PlayerStateName state) {
            playerStates[playerState].ExitState();
            CheckGrounded();
            playerStates[state].EnterState(this);
        }

        private IPlayerState CurrentState() {
            return playerStates[playerState];
        }

        //sprite orientation. returns true if orientation changed

        public bool AdjustFacing(float horizontalInput) {
            if (horizontalInput > 0 && !isFacingRight || horizontalInput < 0 && isFacingRight) {
                isFacingRight = !isFacingRight;
                Vector3 existingColliderLocation = bodyCollider.transform.position;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                Vector3 colliderOffset =  existingColliderLocation - bodyCollider.transform.position;
                if (!isFacingRight) {cameraControls?.SetPlayerFocusXOffset(-colliderOffset.x) ;} else cameraControls?.SetPlayerFocusXOffset(0);
                transform.position = new Vector3 (transform.position.x + colliderOffset.x, transform.position.y + colliderOffset.y, transform.position.z);
                return true;
            }
            return false;
        }

        //collision functions

        //check if player is grounded
        public (bool, bool, List<RaycastHit2D>) CheckGrounded() {
            bool wasGrounded = isGrounded;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Ground", "PhysicalHazards"));
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            //RaycastHit2D hit = Physics2D.CircleCast(footCollider.position, footCollider.gameObject.GetComponent<Collider2D>().bounds.extents.x, new Vector3(0, -1, 0), groundCheckDistance, LayerMask.GetMask("Ground"));
            int hitCount = footCollider.gameObject.GetComponent<Collider2D>().Cast(new Vector3(0, -1, 0), filter, hits, groundCheckDistance, true);
            Debug.DrawRay(footCollider.position, transform.TransformDirection(new Vector3(0, -groundCheckDistance, 0)), Color.yellow);
            isGrounded = hitCount > 0;
            return (isGrounded, wasGrounded, hits);
        }

        public void LeavesGround() {
            isGrounded = false;
        }

        // check if the player has a wall in front of them.
        public (bool, List<RaycastHit2D>) CheckWall() {
            float castDirection = isFacingRight ? 1 : -1;
            (bool hasWall, List<RaycastHit2D> hits) = CheckWall(1);
            return (hasWall, hits);
        }

        public (bool, List<RaycastHit2D>) CheckWall(float castDirection) {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Ground"));
            float numHits = bodyCollider.gameObject.GetComponent<Collider2D>().Cast(Vector2.right * castDirection, filter, hits, wallCheckDistance, true);
            return (numHits > 0, hits);
        }

        // check if the player's head is touching the ceiling.
        public (bool, RaycastHit2D) CheckHeadBump() {
            RaycastHit2D hit = Physics2D.Raycast(headCollider.position, Vector3.up, ceilingCheckDistance, LayerMask.GetMask("Ground"));
            Debug.DrawRay(footCollider.position, transform.TransformDirection(new Vector3(0, ceilingCheckDistance, 0)), Color.yellow, 0.1f);
            if (hit.collider != null) {
                Debug.Log("Ouch! My fucking head!");
                return (true, hit);
            }
            return (false, hit);
        }

        //Slip colliders are placed below the body collider.
        //They only touch the ground if the legs are dangling over a ledge or walking on a very sharp incline.
        //individual state scripts determine how to respond to this.
        public (bool, bool) CheckSlipColliders() {
            RaycastHit2D hitRight = Physics2D.Raycast(rightSlipCollider.position, new Vector3(0, -1, 0), slipCheckDistance, LayerMask.GetMask("Ground"));
            RaycastHit2D hitLeft = Physics2D.Raycast(leftSlipCollider.position, new Vector3(0, -1, 0), slipCheckDistance, LayerMask.GetMask("Ground"));
            return (hitLeft.collider != null, hitRight.collider != null );
        }

        // momentum
        public void AddPlayerMomentum(EntityMomentum momentum) {
            playerMomentum.Add(momentum);
            CurrentState().HandleAddedPlayerMomentum(momentum);
        }

        //used occasionally by states to add their own movement as momentum before transitioning to another state.
        public void AddPlayerMomentum(EntityMomentum momentum, bool doNotHandle) {
            if (!doNotHandle) AddPlayerMomentum(momentum);
            else playerMomentum.Add(momentum);
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
            if (keyDown && playerState == PlayerStateName.ready) {
                if (itemInHand != null) {
                    DropItems();
                } else if (itemInHand == null) {
                    CheckForItems();
                }
            }
        }

        // Item usage functions

        void useItem(GameObject target) {
            playerState = PlayerStateName.working;
            itemUsageBeganFrame = Time.frameCount;
            itemUsageTarget = target;
            itemUsageSound = itemInHand.itemUsageSound;
            if (itemInHand.usageAnimation != "") {
                animator.SetBool(itemInHand.usageAnimation, true);
            }
        }

        void updateItemUsage(int frameCount) {
            if (playerState == PlayerStateName.working && frameCount > itemUsageBeganFrame + itemInHand.usageFrames) {
                if (itemInHand.usageAnimation != "") {
                    animator.SetBool(itemInHand.usageAnimation, false);
                }
                itemInHand.ItemUsed(this, itemUsageTarget);
                if (itemInHand.isConsumed) {
                    DropItems(true);
                }
                itemUsageTarget = null;
                playerState = PlayerStateName.ready;
            } else if (playerState != PlayerStateName.working) {
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
            if (keyDown && playerState == PlayerStateName.ready) {
                if (itemInHand != null) {
                    (bool canUse, GameObject target) = canUseItem(itemInHand);
                    if (canUse) {
                        useItem(target);
                    }
                }
            } else if (keyDown && playerState == PlayerStateName.working) {
                playerState = PlayerStateName.ready;
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
            playerState = PlayerStateName.ready;
            activatableInUse = null;
        }
        

        void ActivateInput(bool keyDown) {
            if (keyDown && playerState == PlayerStateName.aiming && activatableInUse != null && activatableInUse.canManuallyDeactivate) {
                activatableInUse.Deactivate(this);
                DetatchFromActivatable();
            } else if (keyDown && playerState == PlayerStateName.ready) {
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

        //exit the current state;
        //enter the hitstun state.
        public void PlayerTakesDamage() {
            if (IsPlayerInvulnerable()) {
                return;
            }
            needsHitSound = true;
            hitOnframe = Time.frameCount;
            playerState = PlayerStateName.hitstun;
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
            if (playerState == PlayerStateName.hitstun && frameCount > hitStunFrames + hitOnframe) {
                playerState = PlayerStateName.ready;
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), false);
            }
        }

        /*
        //TODO move to state machine
        void SoundUpdate() {
            // play walking sound when moving in the ready state on the ground
            if (!game.sound.IsPlaying("Walk") && playerState == PlayerStateName.ready && speed != 0 && isGrounded) {
                game.sound.Play("Walk"); 
            } else if (game.sound.IsPlaying("Walk") && (playerState != PlayerStateName.ready || speed == 0 || !isGrounded)) {
                game.sound.Stop("Walk");
            }

            // play the working sound when working;
            if (playerState == PlayerStateName.working && itemUsageSound != "" && !game.sound.IsPlaying(itemUsageSound)) {
                game.sound.Play(itemUsageSound);
            }
            
            if (needsHitSound) {
                game.sound.Play("Hit");
                needsHitSound = false;
            }

            if (health == 1 && !game.sound.IsPlaying("LowHP")) {
                game.sound.Play("LowHP"); 
            }

        }*/



        void InputUpdate(float deltaTime) {
            // get input
            bool playerStateWasAiming = playerState == PlayerStateName.aiming;
            bool activateKeyDown = Input.GetKeyDown(KeyCode.F);
            ActivateInput(activateKeyDown);
            if (playerStateWasAiming) {
                return;
            }
            // Camera Toggles
            if (Input.GetKeyDown(KeyCode.C)) {
                cameraControls?.ToggleShipView();
            }

           
            CurrentState().StateInput();
        }

        void Update() {
            float deltaTime = Time.deltaTime;
            int frameCount = Time.frameCount;
            HitStunUpdate(frameCount);
            InputUpdate(deltaTime);
            updateItemUsage(frameCount);

            // update the current state
            CurrentState().UpdateState();

        }

        void Start() {
            // enter the ready state
            playerState = PlayerStateName.ready;
            playerStates[playerState].EnterState(this);
        }

    }
}

