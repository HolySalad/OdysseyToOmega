using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

namespace SpaceBoat.HazardManagers {
    public class SpaceRock : MonoBehaviour
    {
        // Create a rock from the prefab with the necessary.
    
        [SerializeField] private Sprite[] rockSprites;

        private Vector2 velocity;
        private float height;
        private float scale;

        private Destructable destructable;

        public void Awake() {
            destructable = GetComponent<Destructable>();
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

        void OnCollisionEnter2D(Collision2D collision) {
            //Debug.Log("Rock hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                //Debug.Log("Rock Reached the End of the Map");
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                GameModel.Instance.player.PlayerTakesDamage();
                Debug.Log("Rock hit player");
                //TODO add small knockback?
                destructable.Destruct();
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !collision.gameObject.tag.Equals("Platforms")) {
                destructable.Destruct();
            }
        }

        IEnumerator LaunchAfterTime(float time) {
            yield return new WaitForSeconds(time);
            Vector3 scaleVec = new Vector3(scale, scale, 1);
            transform.localScale = scaleVec;
            transform.position = new Vector3(transform.position.x, height, transform.position.z);
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = rockSprites[Random.Range(0, rockSprites.Length)];
            spriteRenderer.transform.localScale = new Vector2(scale * 0.5f, scale * 0.5f);
            CircleCollider2D collider = GetComponent<CircleCollider2D>();
            collider.transform.localScale = scaleVec;
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.AddTorque(1);
            rb.velocity = velocity;
            FindObjectOfType<SoundManager>().Oneshot("RockWhoosh_0"); 
        }
    }
}