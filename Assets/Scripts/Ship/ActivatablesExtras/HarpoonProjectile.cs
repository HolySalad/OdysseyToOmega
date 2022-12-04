using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship 
{    public class HarpoonProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float impactTime = 0.1f;
        [SerializeField] private float reelInDelay = 0.5f;

        private Vector3 directionFired;
        private Rigidbody2D rbHit;
        private Rigidbody2D rbHarpoon;

        public void Awake() {
            rbHarpoon = GetComponent<Rigidbody2D>();
        }

        public void Fire(Vector3 direction) {
            Debug.DrawRay(transform.position, direction*100, Color.red, 5f);
            rbHarpoon.velocity = direction * speed;
            directionFired = direction;
        }

        // reel the harpoon in towards the ship.
        // when 
        IEnumerator ReelInHarpoon() {
            SoundManager.Instance.Play("HarpoonHit");
            // share velocity on impact
            Vector3 newVelocity = rbHit.velocity + rbHarpoon.velocity;
            rbHit.velocity = newVelocity;
            rbHarpoon.velocity = newVelocity;
            yield return new WaitForSeconds(impactTime);
            newVelocity = newVelocity/4;
            rbHit.velocity = newVelocity;
            rbHarpoon.velocity = newVelocity;
            yield return new WaitForSeconds(reelInDelay-impactTime);
            SoundManager.Instance.Play("PullHarpoon");
            newVelocity = newVelocity + (directionFired * -speed/2);
            rbHarpoon.velocity = newVelocity;
            rbHit.velocity = newVelocity;
        }

        public void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                Debug.Log("Harpoon hit map bounds");
                Destroy(this.gameObject);
            } else  if (collision.gameObject.layer == LayerMask.NameToLayer("PhysicalHazards")) {

                Destructable destructable = collision.gameObject.GetComponent<Destructable>();
                if (destructable != null) {
                    Debug.Log("Harpoon hit a physical hazard with a destruction behaviour");
                    destructable.Destruct();
                } else {
                    Debug.Log("Harpoon hit a physical hazard without a destruction behaviour");
                    Destroy(collision.gameObject);
                }
            }
        }


    }
}