using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers.MeteorShowerSubclasses {
    public class Meteorite : MonoBehaviour
    {
        [SerializeField] private Sprite[] meteorSprites;

        private float speed;

        private MeteorShower meteorShower;
        private GameObject target;
        private Destructable destructable;
        private Rigidbody2D rb;

        public void Awake() {
            destructable = GetComponent<Destructable>();
            rb = GetComponent<Rigidbody2D>();
        }

        public float SetupMeteor(MeteorShower meteorShower, float speed, Vector3 startingPosition, GameObject targetSail, float soundTime, bool supressSound = false) {
            this.meteorShower = meteorShower;
            //define a vector from the starting position to the target sail
            target = targetSail;
            Vector3 targetVector = targetSail.transform.position - startingPosition;
            // figure out the time it will take to get there
            float timeToTarget = Vector3.Distance(startingPosition, targetSail.transform.position) / speed;
            float launchDelay = soundTime - timeToTarget - 0.1f;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = meteorSprites[Random.Range(0, meteorSprites.Length)];
            this.speed = speed;
            if (!supressSound) SoundManager.Instance.Oneshot("MeteorWhoosh_0"); 
            StartCoroutine(FireMeteor(launchDelay));
            destructable.AddDestroyCallback(() => {
                meteorShower.meteorHit();
                Debug.Log("Meteorite Destroyed");
            });
            return launchDelay;
        }

        IEnumerator UpdateVelocity() {
            while (true) {
                Vector3 targetVector = target.transform.position - transform.position;
                if (targetVector.magnitude < 0.3f) yield break;
                rb.velocity = new Vector2(targetVector.normalized.x*speed, targetVector.normalized.y*speed);
                yield return null;
            }
        }

        public IEnumerator FireMeteor(float timeToTarget) {
            Debug.Log("Meteor launch in "+timeToTarget);
            yield return new WaitForSeconds(timeToTarget);
            Debug.Log("Meteor launched");
            StartCoroutine(UpdateVelocity());
        }

        void Destruct(bool viaDestructable) {
            if (viaDestructable) {
                destructable.Destruct();
            } else {
                Destroy(this.gameObject);
                meteorShower.meteorHit();
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Meteorite OnTriggerEnter2D");
            if (other.gameObject == target) {
                Ship.SailsActivatable sail = other.gameObject.GetComponent<Ship.SailsActivatable>();
                if (!sail.isBroken) sail.Break();
                Destruct(false);
                SoundManager.Instance.Play("MeteorImpact"); 
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            Debug.Log("Rock hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                Debug.LogWarning("Meteor Reached the End of the Map. This shouldn't happen, they are supposed to always hit sails.");
                Destruct(false);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                Debug.Log("Meteor hit player");
                GameModel.Instance.player.PlayerTakesDamage();
                Destruct(true);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !collision.gameObject.tag.Equals("Platforms")
                && !collision.gameObject.tag.Equals("SpaceRocks")) {
                Destruct(true);
            }
        }
    }
}