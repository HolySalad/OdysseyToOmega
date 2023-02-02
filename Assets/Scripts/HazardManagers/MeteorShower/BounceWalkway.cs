using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers.MeteorShowerSubclasses {    
    public class BounceWalkway : MonoBehaviour, Environment.IBouncable
    {
        
        [SerializeField] private int bounceCooldown = 12;
        [SerializeField] private float bounceForceMult = 0.5f;
        [SerializeField] private float bounceForceDecay = 15f;
        [SerializeField] private float reboundForceMult = 0.5f;
        [SerializeField] private float reboundDecayMult = 0.5f;
        [SerializeField] private GameObject parentRock;

        private int bounceTimer = 0;
        private Rigidbody2D rb;
        private float originalVerticalVelocity;
        private float lastBounceForce = 0f;
        private float lastReboundForce = 0f;

        void Start() {
            rb = parentRock.GetComponent<Rigidbody2D>();
            originalVerticalVelocity = rb.velocity.y;
        }

        public bool Bounce(Player player) {
            float playerVelocity = player.gameObject.GetComponent<Rigidbody2D>().velocity.y;
            if (bounceTimer > 0 || player.currentPlayerStateName != PlayerStateName.ready || playerVelocity > 0) {
                return false;
            }
            lastBounceForce = playerVelocity* bounceForceMult;
            originalVerticalVelocity = rb.velocity.y-lastReboundForce;
            rb.velocity = new Vector2(rb.velocity.x, originalVerticalVelocity + lastBounceForce);
            if (playerVelocity < 0) player.ForceJump(false, true, true);
            bounceTimer = bounceCooldown;
            return true;
        }

        // Update is called once per frame
        void Update()
        {
            if (bounceTimer > 0) {
                bounceTimer--;
            }
            if (rb.velocity.y < originalVerticalVelocity) {
                float newVelocity = Mathf.Min(originalVerticalVelocity, rb.velocity.y + (bounceForceDecay * Time.deltaTime));
                if (newVelocity == originalVerticalVelocity) {
                    lastReboundForce = -lastBounceForce * reboundForceMult;
                    newVelocity = originalVerticalVelocity + (lastReboundForce);
                }
                rb.velocity = new Vector2(rb.velocity.x, newVelocity);
            } else if (rb.velocity.y > originalVerticalVelocity) {
                float newVelocity = Mathf.Max(originalVerticalVelocity, rb.velocity.y - (bounceForceDecay *reboundDecayMult * Time.deltaTime));
                rb.velocity = new Vector2(rb.velocity.x, newVelocity);
            }
        }
    }
}
