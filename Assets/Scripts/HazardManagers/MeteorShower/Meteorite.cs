using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers.MeteorShowerSubclasses {
    public class Meteorite : MonoBehaviour
    {
        [SerializeField] private Sprite[] meteorSprites;

        private float speed;

        private GameObject target;
        private Destructable destructable;
        private Rigidbody2D rb;

        public void Awake() {
            destructable = GetComponent<Destructable>();
            rb = GetComponent<Rigidbody2D>();
        }

        public void SetupMeteor(float speed, Vector3 startingPosition, GameObject targetSail, float soundTime) {
            //define a vector from the starting position to the target sail
            target = targetSail;
            Vector3 targetVector = targetSail.transform.position - startingPosition;
            // figure out the time it will take to get there
            float timeToTarget = Vector3.Distance(startingPosition, targetSail.transform.position) / speed;
            float launchDelay = soundTime - timeToTarget - 0.1f;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = meteorSprites[Random.Range(0, meteorSprites.Length)];
            this.speed = speed;
            SoundManager.Instance.Oneshot("MeteorWhoosh_0"); 
            StartCoroutine(FireMeteor(launchDelay));
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

        void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Meteorite OnTriggerEnter2D");
            if (other.gameObject == target) {
                Ship.SailsActivatable sail = other.gameObject.GetComponent<Ship.SailsActivatable>();
                if (!sail.isBroken){sail.Break();
                Destroy(this.gameObject);}
                SoundManager.Instance.Play("MeteorImpact"); 
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            Debug.Log("Rock hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                Debug.LogWarning("Meteor Reached the End of the Map. This shouldn't happen, they are supposed to always hit sails.");
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                Debug.Log("Meteor hit player");
                GameModel.Instance.player.PlayerTakesDamage();
                destructable.Destruct();
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !collision.gameObject.tag.Equals("Platforms")
                && !collision.gameObject.tag.Equals("SpaceRocks")) {
                destructable.Destruct();
            }
        }
    }
}