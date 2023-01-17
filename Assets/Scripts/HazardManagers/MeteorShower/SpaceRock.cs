using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

namespace SpaceBoat.HazardManagers {
    public class SpaceRock : MonoBehaviour
    {
        // Create a rock from the prefab with the necessary.
    
        [SerializeField] private Sprite[] rockSprites;
        [SerializeField] private float rockBobBaseSpeed = 1f;
        [SerializeField] private float rockBobDownAcceleration = 0.5f;
        [SerializeField] private float rockReboundSpeed = 1.2f;
        [SerializeField] private float rockReboundAcceleration = 0.3f;
        [SerializeField] private float playerUpwardForceOnRebound = 20f;
        [SerializeField] private float rockBobBaseDistance = 0.6f;
        [SerializeField] private float rockBobDistanceFalloff = 0.6f;

        [SerializeField] private float bobBobMinDistance = 0.1f;


        private Vector2 velocity;
        private float height;
        private float scale;

        private Destructable destructable;
        private SpriteRenderer spriteRenderer;
        private Transform spriteTransform;
        private RockBouncer rockBounce;
        private bool isBobbing = false;
        private bool isRebounding = false;

        private float bobStartDistance = 0f;
        private Coroutine bobCoroutine;

        private float startHeight;
        private bool hasReachedStartHeight = true;


        public void Awake() {
            destructable = GetComponentInChildren<Destructable>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spriteTransform = spriteRenderer.transform;
        }

        public void SetupRock(float speed, float angle, float scale, float spawnHeight, float launchTime) {
            // send a rock flying at the player
            this.velocity = new Vector2(-speed * Mathf.Cos(Mathf.Deg2Rad *angle), speed * Mathf.Sin(Mathf.Deg2Rad * angle));
            this.height = spawnHeight;
            this.scale = scale;
            float launchtime = launchTime - Time.timeSinceLevelLoad;
            Debug.Log("Launching a rock at " + launchtime + " with velocity " + velocity + " and height " + height + " and scale " + scale);
           StartCoroutine(LaunchAfterTime(launchtime));
        }

        public void StartRockBobbing(Collision2D coll) {
            if (isBobbing) {
                if (bobCoroutine != null) {
                    destructable.RemoveCoroutine(bobCoroutine);
                    StopCoroutine(bobCoroutine);
                }
                float distanceAfterFalloff = bobStartDistance*rockBobDistanceFalloff;
                if (distanceAfterFalloff < bobBobMinDistance) {
                }
                bobCoroutine = StartCoroutine(BobRock(bobStartDistance, rockBobBaseSpeed * (distanceAfterFalloff/bobStartDistance)));
                bobStartDistance = distanceAfterFalloff;
            } else if (isRebounding) {
                float bobVelocity = coll.gameObject.GetComponent<Rigidbody2D>().velocity.y + rockReboundSpeed;
                bobStartDistance = rockBobBaseDistance / bobVelocity;
                bobCoroutine = StartCoroutine(BobRock(bobStartDistance, bobVelocity));
            } else {
                bobStartDistance = rockBobBaseDistance;
                bobCoroutine = StartCoroutine(BobRock(bobStartDistance, rockBobBaseSpeed));
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            //Debug.Log("Rock hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                //Debug.Log("Rock Reached the End of the Map");
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar") && rockBounce.canBobStart) {
                Collider2D coll = GetComponentInChildren<BoxCollider2D>();
                ContactPoint2D[] contacts = new ContactPoint2D[10];
                ContactFilter2D filter = new ContactFilter2D();
                filter.SetLayerMask(LayerMask.GetMask("PlayerChar"));
                coll.GetContacts(filter, contacts);
                if (contacts[0].collider != null && contacts[0].collider.gameObject.GetComponent<Player>() != null) {
                    //do nothing
                } else {
                    GameModel.Instance.player.PlayerTakesDamage();
                    //GameModel.Instance.player.AddMomentum(new Vector2(velocity.x, 0));
                    destructable.Destruct(this.gameObject);
                }
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !collision.gameObject.tag.Equals("Platforms")) {
                destructable.Destruct(this.gameObject);
            }
        }

        IEnumerator LaunchAfterTime(float time) {
            yield return new WaitForSeconds(time);
            Vector3 scaleVec = new Vector3(scale, scale, 1);
            transform.localScale = scaleVec;
            transform.position = new Vector3(transform.position.x, height, transform.position.z);
            spriteRenderer.sprite = rockSprites[Random.Range(0, rockSprites.Length)];
            BoxCollider2D boxCollider = GetComponentInChildren<BoxCollider2D>();
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = velocity;
            FindObjectOfType<SoundManager>().Oneshot("RockWhoosh_0"); 
            destructable.RegisterCoroutine(StartCoroutine(SpinRock()));
            rockBounce = transform.GetChild(0).gameObject.AddComponent<RockBouncer>();
            rockBounce.Init(this, scale, playerUpwardForceOnRebound);
        }


        IEnumerator SpinRock() {
            while (true) {
                spriteTransform.Rotate(0, 0, 5);
                yield return new WaitForSeconds(0.01f);
            }
        }

        IEnumerator BobRock(float bobDistance, float speed) {
            if (hasReachedStartHeight) {
                startHeight = transform.position.y;
            }
            hasReachedStartHeight = false;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            isBobbing = true;
            Debug.Log("Bobbing started at Y " + transform.position.y + " with a velocity of " + rb.velocity.y + " and a start height of " + startHeight + " and a bob distance of " + bobDistance);
            while (transform.position.y > startHeight - bobDistance/scale && isBobbing == true) {
                if (-rb.velocity.y < rockBobBaseSpeed) {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y - rockBobDownAcceleration);
                }
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("Bobbing finished at Y " + transform.position.y + " with a velocity of " + rb.velocity.y + " and a start height of " + startHeight + " and a bob distance of " + bobDistance);
            isBobbing = false;
            isRebounding = true;
            while (transform.position.y < startHeight && isRebounding == true) {
                if (rb.velocity.y < rockReboundSpeed) {
                    rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + rockReboundAcceleration);
                }
                yield return new WaitForEndOfFrame();
            }
            Debug.Log("Rebounding finished at Y " + transform.position.y + " with a velocity of " + rb.velocity.y + " and a start height of " + startHeight + " and a bob distance of " + bobDistance);
            hasReachedStartHeight = true;
            rb.velocity  = new Vector2(rb.velocity.x, 0);
            isRebounding = false;
            yield return null;
        }
    }

    public class RockBouncer : MonoBehaviour {
        public bool canBobStart { get; private set; }
        private SpaceRock rock;
        private float knockBack;
        private float scale;

        public void Init(SpaceRock rock, float scale, float knockBack) {
            canBobStart = true; 
            this.rock = rock;
            this.knockBack = knockBack;
            this.scale = scale;
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                if (rock != null && canBobStart) {
                    rock.StartRockBobbing(collision);
                    canBobStart = false;
                    Player player = GameModel.Instance.player;
                    //player.forceInstantHalfJump();
                    //player.AddMomentum(new Vector2(0, knockBack * scale));
                }
            }
        }

        void OnCollisionExit2D(Collision2D collision) {
            if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                canBobStart = true;
            }
        }
    }
}