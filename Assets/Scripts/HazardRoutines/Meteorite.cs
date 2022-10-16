using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Hazards{
    public class Meteorite : MonoBehaviour
    {

        [SerializeField] private Sprite[] meteorSprites;

        public void SetupMeteor(float speed, Vector3 startingPosition, GameObject targetSail) {
            //define a vector from the starting position to the target sail
            Vector2 targetVector = targetSail.transform.position - startingPosition;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = meteorSprites[Random.Range(0, meteorSprites.Length)];
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(targetVector.normalized.x*speed, targetVector.normalized.y*speed);
        }

        void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Meteorite OnTriggerEnter2D");
            int layer = other.gameObject.layer;
            if (layer == LayerMask.NameToLayer("Sails")) {
                Sails sail = other.gameObject.GetComponent<Sails>();
                if (!sail.IsBroken){sail.Break();
                Destroy(this.gameObject);}
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            Debug.Log("Rock hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("EndOfMapLeft")) {
                Debug.LogWarning("Meteor Reached the End of the Map. This shouldn't happen, they are supposed to always hit sails.");
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                Player.PlayerLogic player = collision.gameObject.GetComponent<Player.PlayerLogic>();
                Debug.Log("Meteor hit player");
                player.PlayerTakesDamage(1);
                Destroy(this.gameObject);
                //TODO add small knockback?
                //TODO rock breaking animation.
                //TODO sound
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !collision.gameObject.tag.Equals("Platforms")) {
                Destroy(this.gameObject);
                //TODO rock breaking animation.
                //TODO sound
            }
        }
    }
}