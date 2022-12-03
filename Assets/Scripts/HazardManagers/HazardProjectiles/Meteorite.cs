using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {
    public class Meteorite : MonoBehaviour
    {
         [SerializeField] private Sprite[] meteorSprites;

        private Vector2 velocity;

        private GameObject target;

        public void SetupMeteor(float speed, Vector3 startingPosition, GameObject targetSail, float soundTime) {
            //define a vector from the starting position to the target sail
            target = targetSail;
            Vector3 targetVector = targetSail.transform.position - startingPosition;
            // figure out the time it will take to get there
            float timeToTarget = Vector3.Distance(startingPosition, targetSail.transform.position) / speed;
            float launchDelay = soundTime - timeToTarget - 0.1f;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = meteorSprites[Random.Range(0, meteorSprites.Length)];
            velocity = new Vector2(targetVector.normalized.x*speed, targetVector.normalized.y*speed);
            SoundManager.Instance.Oneshot("MeteorWhoosh_0"); 
            StartCoroutine(FireMeteor(launchDelay));
        }

        public IEnumerator FireMeteor(float timeToTarget) {
            Debug.Log("Meteor launch in "+timeToTarget);
            yield return new WaitForSeconds(timeToTarget);
            Debug.Log("Meteor launched");
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = velocity;
        }

        void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Meteorite OnTriggerEnter2D");
            if (other.gameObject == target) {
                Ship.Sails sail = other.gameObject.GetComponent<Ship.Sails>();
                if (!sail.isBroken){sail.Break();
                Destroy(this.gameObject);}
                SoundManager.Instance.Play("MeteorImpact"); 
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            Debug.Log("Rock hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("EndOfMapLeft")) {
                Debug.LogWarning("Meteor Reached the End of the Map. This shouldn't happen, they are supposed to always hit sails.");
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                Debug.Log("Meteor hit player");
                GameModel.Instance.player.PlayerTakesDamage();
                Destroy(this.gameObject);
                //TODO add small knockback?
                //TODO rock breaking animation.
                FindObjectOfType<SoundManager>().Play("MeteorImpact"); 
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !collision.gameObject.tag.Equals("Platforms")) {
                Destroy(this.gameObject);
                //TODO rock breaking animation.
                FindObjectOfType<SoundManager>().Play("MeteorImpact"); 
            }
        }
    }
}