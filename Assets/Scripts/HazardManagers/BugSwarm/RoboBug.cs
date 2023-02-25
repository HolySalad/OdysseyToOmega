using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;


namespace SpaceBoat.HazardManagers.BugSwarmSubclasses {

    [System.Serializable] public class RoboBugSpriteSet {
        public Sprite generalSprite;
        public Sprite attackSprite;
    }
 
    public class RoboBug : MonoBehaviour
    {
        [SerializeField] private RoboBugSpriteSet[] spriteSets;
        [SerializeField] private float bugRotation = 5f;

        [SerializeField] private float hoverMovementSpeed = 5f;
        [SerializeField] private float travellingMovementSpeed = 8f;
        [SerializeField] private float hoverMovementVerticalSinFunctionAmplitude = 1f;
        [SerializeField] private float hoverMovementMaxCycleTime = 5f;
        [SerializeField] private float hoverMovementMinCycleTime = 2f;
        [SerializeField] private float maxAttackSpeed = 20f;
        [SerializeField] private float noAbortingAttackAfterSpeedProportion = 0.8f;
        [SerializeField] private float attackAcceleration = 10f;
        [SerializeField] private float maxAttackVectorChangePercent = 0.1f;
        [SerializeField] private float minAttackVectorChangePercent = 0.1f;
        [SerializeField] private float attackVectorMagDifferenceForAbort = 0.1f;
        [SerializeField] private float explosionRadius = 2f;
        [SerializeField] private float attackTimeoutTime = 2f;
        [SerializeField] private float minAttackWaitTime = 0.2f;
        [SerializeField] private float attackCooldown = 1.5f;

        [SerializeField] private float bombDropHeight = 4f;
        [SerializeField] private GameObject bombPrefab;
        [SerializeField] private SpriteRenderer bombSprite;
        [SerializeField] private GameObject moneyPrefab;
        [SerializeField] private int moneyDropChance = 10;

        
        [SerializeField] private float bugoffTime = 12f;    

        [SerializeField] private GameObject explosionAnimationObject;
        [SerializeField] private Light2D[] atttackModeLights;
        [SerializeField] private SpriteRenderer attackModeJetSprite;


        private BugSwarm swarm;

        public Vector3 targetLocation {get; private set;} = Vector3.zero;
        private Transform exitTarget;
        public bool isAttacking = false;
        private bool hasStartedMoving = false;
        public bool isLeaving = false;
        private bool carriesBomb = false;
        private GameObject bombTarget = null;
        


        private SpriteRenderer spriteRenderer;
        private Rigidbody2D rb;
        private RoboBugSpriteSet currentSpriteSet;

        private GameObject playerTarget;
        private Vector2 attackVector = Vector2.zero;
        private Vector2 startingAttackVector = Vector2.zero;
        private float attackTimeoutTimer = 0f;
        private float attackWaitTimer = 0f;

        private bool reachedTargetLocation = false;
        private bool lookingLeft = true;
        private Vector3 currentMovementOffset = Vector3.zero;
        private float currentSinFunctionDegree = 0f;
        private float currentHoverCycleTime = 0f;

        private float bugOffTimer = 0f;
        private float attackEndedTime = 0f;

        private float currentAttackSpeed = 0f;

        public void SetupBomber(GameObject targetSail) {
            Debug.Log("Setting up "+name+" as a bug bomber");
            bombSprite.enabled = true;
            carriesBomb = true;
            bombTarget = targetSail;
            targetLocation = new Vector3(targetSail.transform.position.x, targetSail.GetComponent<Ship.Activatables.SailsActivatable>().hazardTarget.position.y + bombDropHeight, 0f);
        }

        public void SetupRobobug(BugSwarm swarm, Vector3 targetLocation, Transform exitLocation) {
            this.swarm = swarm;
            this.targetLocation = targetLocation;
            this.exitTarget = exitLocation;
        }

        void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
        }

        void TurnBug(bool lookLeft) {
            if (lookLeft && !lookingLeft) {
                lookingLeft = true;
                transform.rotation = Quaternion.Euler(0f, 0f, -transform.rotation.eulerAngles.z);
                transform.localScale = new Vector3(1f, 1f, 1f);
            } else if (lookingLeft && !lookLeft) {
                lookingLeft = false;
                transform.rotation = Quaternion.Euler(0f, 0f, -transform.rotation.eulerAngles.z);
                transform.localScale = new Vector3(-1f, 1f, 1f);
            } 
        }

        void UpdateBugAngle() {
            if (lookingLeft) {
                if ((int)transform.rotation.eulerAngles.z == (int)bugRotation) return;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 0f, bugRotation), 70f*Time.deltaTime);
            } else {
                if ((int)transform.rotation.eulerAngles.z == (int)-bugRotation) return;
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 0f, -bugRotation), 70f*Time.deltaTime);
            }
        }

        void BugAttackingMovementBehaviour() {
            Vector2 targetVector = playerTarget.transform.position - transform.position;

            if (currentAttackSpeed < maxAttackSpeed*noAbortingAttackAfterSpeedProportion) {    
                attackVector = (attackVector + ((targetVector - attackVector) *maxAttackVectorChangePercent*Time.deltaTime)).normalized;
                if (Vector2.Distance(attackVector.normalized, startingAttackVector.normalized) > attackVectorMagDifferenceForAbort) {
                    Debug.Log("Bug "+name+" in attack phase: attack vector has changed too much, aborting attack! attack vector is: " + attackVector + ", the starting attack vector is: " + startingAttackVector + " ;The difference magnitude is: " +Vector2.Distance(attackVector.normalized, startingAttackVector.normalized));
                    attackTimeoutTimer = 0f;
                    isAttacking = false;
                    hasStartedMoving = false;
                    targetLocation = transform.position;
                    attackEndedTime = Time.time;
                    return;
                }
            } else {
                attackVector = (attackVector + ((targetVector - attackVector) *minAttackVectorChangePercent*Time.deltaTime)).normalized;
            }

            float angle = Mathf.Atan2(attackVector.y, attackVector.x)*Mathf.Rad2Deg;
            if (angle < 0) {
                angle += 360;
            }
            //TODO if the angle is between 90 and 270, the bug should look Right;
            if (lookingLeft) {
                angle += bugRotation;
                angle -= 180;
            } else {
                angle -= bugRotation;
            }
            Debug.Log("Bug "+name+" in attack phase: attack angle is: " + angle + " degrees (looking left: " + lookingLeft + "), the current angle is: " + transform.rotation.eulerAngles.z + " degrees");
            attackWaitTimer += Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0f, 0f, angle), 70f*Time.deltaTime);
            if (!hasStartedMoving && (int)transform.rotation.eulerAngles.z == (int)angle) {
                Debug.Log("Bug reached target angle!");
                if (attackWaitTimer > minAttackWaitTime) hasStartedMoving = true;
            }
            if (hasStartedMoving) {
                attackTimeoutTimer += Time.deltaTime;
                if (attackTimeoutTimer >= attackTimeoutTime) {
                    currentAttackSpeed = Mathf.Max(currentAttackSpeed-(attackAcceleration*1.4f*Time.deltaTime), 0);
                    if (currentAttackSpeed <= hoverMovementSpeed) {
                        attackTimeoutTimer = 0f;
                        isAttacking = false;
                        hasStartedMoving = false;
                        targetLocation = transform.position;
                        attackEndedTime = Time.time;
                        TurnBug(lookingLeft);
                        currentSinFunctionDegree = lookingLeft ? 180 : 0;
                        return;
                    }
                } else {
                    currentAttackSpeed = Mathf.Min(currentAttackSpeed+(attackAcceleration*Time.deltaTime), maxAttackSpeed);
                }
                rb.velocity = attackVector*currentAttackSpeed;
            } else {
                //check if we're very close to the ground
                if (Physics2D.Raycast(transform.position, Vector2.down, 2f, LayerMask.GetMask("Ground"))) {
                    rb.velocity = Vector2.zero;
                } else {
                    rb.velocity = hoverMovementSpeed*Vector2.down;
                }
            }
        }

        void BugHoverMovementBehaviour() {
            if (currentSinFunctionDegree >= 360f) {
                currentHoverCycleTime = Random.Range(hoverMovementMinCycleTime, hoverMovementMaxCycleTime);
                currentSinFunctionDegree = 0f;
            }
            currentSinFunctionDegree += 360f/currentHoverCycleTime*Time.deltaTime;
            Vector3 originalPosition = transform.position - currentMovementOffset;
            currentMovementOffset = new Vector3(0f, Mathf.Sin(currentSinFunctionDegree*Mathf.Deg2Rad)*hoverMovementVerticalSinFunctionAmplitude, 0f);
            transform.position = originalPosition + currentMovementOffset;
        }

        void BugTravelMovementBehaviour() {
            Vector3 movementVector = targetLocation - transform.position;
            if (movementVector.x > 0) {
                TurnBug(false);
            } else {
                TurnBug(true);
            }
            rb.velocity = movementVector.normalized*travellingMovementSpeed;
        }

        void BugPatrolMovementBehaviour() {
            if (currentSinFunctionDegree >= 180 && !lookingLeft) {
                TurnBug(true);
                rb.velocity = Vector2.left * hoverMovementSpeed;
            } else if (currentSinFunctionDegree < 180 && lookingLeft) {
                TurnBug(false);
                rb.velocity = Vector2.right * hoverMovementSpeed;
            }
        }

        void BugBomberMovementBehaviour() {
            if ((int)transform.position.x - (int)currentMovementOffset.x == (int)targetLocation.x) {
                reachedTargetLocation = true;
                bombSprite.enabled = false;
                GameObject bomb = Instantiate(bombPrefab, bombSprite.transform.position, bombSprite.transform.rotation);
                bomb.GetComponent<BugBomb>().SetTargetSail(bombTarget);
                isLeaving = true;
                carriesBomb = false;
                attackEndedTime = Time.time;
            } else {
                Vector3 movementVector = targetLocation - transform.position;
                rb.velocity = movementVector.normalized*travellingMovementSpeed;
            }
        }

        void BugLeavingMovementBehaviour() {
            if (transform.position.x < exitTarget.position.x) {
                swarm?.RemoveBugFromSwarm(this);
                Destroy(gameObject);
                return;
            }
            Vector3 movementVector = exitTarget.position - transform.position;
            if (!lookingLeft) {
                TurnBug(true);
            }
            rb.velocity = movementVector.normalized*travellingMovementSpeed;
        }

        public void DropMoney() {
            Instantiate(moneyPrefab, transform.position, Quaternion.identity);
        }


        IEnumerator MoveBug() {
            if (explosionAnimationObject.activeSelf) {
                rb.velocity = Vector2.zero;
                yield break;
            }
            while (true) {
                if (isAttacking) {
                    BugAttackingMovementBehaviour();
                } else if (carriesBomb) {
                    UpdateBugAngle();
                    BugHoverMovementBehaviour();
                    BugBomberMovementBehaviour();
                } else if (isLeaving) {
                    UpdateBugAngle();
                    BugHoverMovementBehaviour();
                    BugLeavingMovementBehaviour();
                } else {
                    UpdateBugAngle();
                    BugHoverMovementBehaviour();
                    if (reachedTargetLocation || (int)transform.position.x - (int)currentMovementOffset.x == (int)targetLocation.x) {
                        reachedTargetLocation = true;
                        BugPatrolMovementBehaviour();
                        bugOffTimer += Time.deltaTime;
                        if (bugOffTimer >= bugoffTime) {
                            isLeaving = true;
                        }
                    } else {
                        BugTravelMovementBehaviour();
                    }
                }
                yield return null;
            }

        }

        IEnumerator UpdateBugVisuals() {
            while (true) {
                
                // if the explosion animation was set off, wait for it to finish before destroying the bug
                if (explosionAnimationObject.activeSelf) {
                    rb.velocity = Vector2.zero;
                    Animator animator = explosionAnimationObject.GetComponent<Animator>();
                    while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f) {
                        yield return null;
                    }
                    Destroy(gameObject);
                    yield break;
                }  

                // if the bug is attacking, set the attack sprite and turn on the attack lights
                if (isAttacking) {
                    spriteRenderer.sprite = currentSpriteSet.attackSprite;
                    attackModeJetSprite.enabled = hasStartedMoving;
                    foreach (Light2D light in atttackModeLights) {
                        light.enabled = true;
                    }
                } else {
                    spriteRenderer.sprite = currentSpriteSet.generalSprite;
                    attackModeJetSprite.enabled = false;
                    foreach (Light2D light in atttackModeLights) {
                        light.enabled = false;
                    }
                }

                yield return null;
            }
        }

        void Start() {
            currentSpriteSet = spriteSets[Random.Range(0, spriteSets.Length)];
            StartCoroutine(UpdateBugVisuals());
            TurnBug(true);
            currentHoverCycleTime = Random.Range(hoverMovementMinCycleTime, hoverMovementMaxCycleTime);
            StartCoroutine(MoveBug());
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.layer == LayerMask.NameToLayer("PhysicalHazards"))
                return;
            if (explosionAnimationObject.activeSelf) return;
            spriteRenderer.enabled = false;
            foreach (Light2D light in atttackModeLights) {
                light.enabled = false;
            }
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            StopCoroutine(MoveBug());
            explosionAnimationObject.SetActive(true);
            if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar") && collision.gameObject.TryGetComponent(out Player playerChar)) {
                playerChar.PlayerTakesDamage();
            }
            swarm?.RemoveBugFromSwarm(this);
            if (collision.gameObject.layer != LayerMask.NameToLayer("MapBounds")) {
                SoundManager.Instance.Play("BugExplosion");
                if (Random.Range(0, 100) < moneyDropChance) {
                    DropMoney();
                }
            }
        }

        bool CheckLineOfSight(Vector3 target) {
            Debug.DrawLine(transform.position, target, Color.red, 1f);
            Debug.Log("Checking line of sight");
            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(LayerMask.GetMask("Ground"));
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            int numHits = Physics2D.Raycast(transform.position, target - transform.position, contactFilter, hits, Vector2.Distance(transform.position, target));
            if (numHits > 0) {
                for (int i = 0; i < numHits; i++) {
                    if (hits[i].collider.CompareTag("Ship")) {
                        return false;
                    }
                }
                return true;
            } else {
                return true;
            }
        }

        void OnTriggerStay2D(Collider2D collider) {
            if (isAttacking) return;
            if (carriesBomb) return;
            if (attackEndedTime + attackCooldown > Time.time) return;
            if (collider.gameObject.layer == LayerMask.NameToLayer("PlayerChar") && collider.gameObject.TryGetComponent(out Player playerChar)) {
                bool lookingInPlayersDirection = (lookingLeft && playerChar.transform.position.x < transform.position.x) || (!lookingLeft && playerChar.transform.position.x > transform.position.x);
               if (lookingInPlayersDirection&& CheckLineOfSight(playerChar.transform.position)) { 
                    Debug.Log(name + " is attacking");
                    SoundManager.Instance.Oneshot("BugAlarm");
                    isAttacking = true;
                    playerTarget = playerChar.gameObject;
                    attackWaitTimer = 0f;
                    attackVector = (playerChar.transform.position - transform.position).normalized;
                    startingAttackVector = attackVector;
                    TurnBug(attackVector.x < 0f);
                }
            }

        }
    }
}