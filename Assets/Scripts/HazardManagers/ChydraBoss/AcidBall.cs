using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers.ChydraBossSubclasses {    
    public class AcidBall : MonoBehaviour
    {
        [SerializeField] private float movementSpeed = 10f;
        [SerializeField] private float arcHeight = 2.5f;

        private Transform target;
        private float midpoint = 0f;
        private bool isFiring = false;
        private Rigidbody2D rb;

        public void Fire(Transform target, Transform origin) {
            rb = GetComponent<Rigidbody2D>();
            this.target = target;
            midpoint = (target.position.x + origin.position.x) / 2f;
            isFiring = true;
        }

        void FixedUpdate() {
            if (!isFiring) {
                return;
            }
            float x = transform.position.x - midpoint;
            float heightChange = -arcHeight * (x)*2;
            rb.velocity = new Vector2(-movementSpeed, heightChange);
        }


        void OnCollisionEnter2D(Collision2D collision) {
            Debug.Log("Rock hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar")) {
                Debug.Log("Meteor hit player");
                GameModel.Instance.player.TakeDamage();
                Destroy(this.gameObject);
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && !collision.gameObject.tag.Equals("Platforms")
                && !collision.gameObject.tag.Equals("SpaceRocks")) {
                Destroy(this.gameObject);
            }
        }


        void OnTriggerEnter2D(Collider2D other) {
            Debug.Log("Meteorite OnTriggerEnter2D");
            if (other.gameObject == target) {
                Ship.Activatables.SailsActivatable sail = other.gameObject.GetComponent<Ship.Activatables.SailsActivatable>();
                if (!sail.isBroken) sail.Break();
                Destroy(this.gameObject);
                SoundManager.Instance.Play("MeteorImpact"); 
            }
        }
    }
}