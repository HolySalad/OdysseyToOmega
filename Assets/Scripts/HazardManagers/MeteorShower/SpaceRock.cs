using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

namespace SpaceBoat.HazardManagers.MeteorShowerSubclasses {
    public class SpaceRock : MonoBehaviour
    {
        // Create a rock from the prefab with the necessary.
    
        [SerializeField] private Sprite[] rockSprites;
        [SerializeField] private AudioClip rockWhoosh;

        private Destructable destructable;
        private SpriteRenderer spriteRenderer;
        private Transform spriteTransform;


        public void Awake() {
            destructable = GetComponentInChildren<Destructable>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            spriteTransform = spriteRenderer.transform;
        }

        public void SetupRock(float speed, float angle, float scale, bool playSound) {
            // send a rock flying at the player
            Vector3 scaleVec = new Vector3(scale, scale, 1);
            transform.localScale = scaleVec;
            spriteRenderer.sprite = rockSprites[Random.Range(0, rockSprites.Length)];
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(-speed * Mathf.Cos(Mathf.Deg2Rad *angle), speed * Mathf.Sin(Mathf.Deg2Rad * angle));
            transform.rotation = Quaternion.Euler(0, 0, -angle);
        }

        void OnCollisionEnter2D(Collision2D collision) {
            //Debug.Log("Rock hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                //Debug.Log("Rock Reached the End of the Map");
                StopAllCoroutines();
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                GameModel.Instance.player.TakeDamage();
                //GameModel.Instance.player.AddMomentum(new Vector2(velocity.x, 0));
                GetComponent<Collider2D>().enabled = false;
                destructable.Destruct(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !collision.gameObject.tag.Equals("Platforms")
                && !collision.gameObject.tag.Equals("SpaceRocks")) {
                GetComponent<Collider2D>().enabled = false;
                destructable.Destruct(this.gameObject);
            }
        }



        IEnumerator SpinRock() {
            while (true) {
                spriteTransform.Rotate(0, 0, 5);
                yield return new WaitForSeconds(0.01f);
            }
        }

    }
}