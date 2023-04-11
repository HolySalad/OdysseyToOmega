using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Enemies.Siren {
    public class SirenOrb : MonoBehaviour
    {
        [SerializeField] private float speed = 8f;
        [SerializeField] private float timeToLive = 6f;
        [SerializeField] private float disappearTime = 1f;
        [SerializeField] private float trackingInterval = 0.5f;
        [SerializeField] private float maxVectorChangePercent = 0.1f;

        private Player player;
        private Rigidbody2D rb;
        private float timeCreated;
        private float timeLastTracked;

        Vector2 GetTrackingVelocity() {
            Vector2 targetVector = player.transform.position - transform.position;
            Vector2 currentVector = rb.velocity;

            return (currentVector + ((targetVector - currentVector) *maxVectorChangePercent)).normalized * speed;
        }

        void Awake() {
            rb = GetComponent<Rigidbody2D>();
        }

        // update the orb's velocity to aim at the player every time the tracking interval has passed.
        // if the orb has been alive for too long, destroy it.
        IEnumerator OrbUpdater() {
            while (Time.time - timeCreated < timeToLive) {
                yield return new WaitForSeconds(trackingInterval);
                rb.velocity = GetTrackingVelocity();
            }
            float currentScale = transform.localScale.x;
            float changePerFrame = (currentScale / disappearTime)*Time.deltaTime;
            while (currentScale > 0) {
                currentScale -= changePerFrame;
                transform.localScale = new Vector3(currentScale, currentScale, currentScale);
                yield return null;
            }
            Destroy(gameObject);
            yield break;
        }

        public void SetupOrb(Player playerTarget) {
            player = playerTarget;
            timeCreated = Time.time;
            rb.velocity = GetTrackingVelocity();
            StartCoroutine(OrbUpdater());
        }

        // if colliding with the ship, break.
        // if colliding with the player, deal damage and break.
        // if colliding with the siren's shield, destroy shield and break.
        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.tag == "Player") {
                player.TakeDamage();
                Destroy(gameObject);
            } else if (collision.gameObject.tag == "SirenShield") {
                Destroy(collision.gameObject);
                Destroy(gameObject);
            } else if (collision.gameObject.tag == "Ship") {
                Destroy(gameObject);
            }
        }
    }
}