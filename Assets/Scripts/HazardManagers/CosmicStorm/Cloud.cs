using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship;

namespace SpaceBoat.HazardManagers {    
    public class Cloud : MonoBehaviour
    {
        
        [SerializeField] private int attemptLightningChance = 10;
        [SerializeField] private float lightningHeightBase = 17f;
        [SerializeField] private GameObject lightningPrefab;
        [SerializeField] private AudioClip lightningSound;
        [SerializeField] private AudioClip chargeSound;

        private CosmicStorm storm;
        private float chargeTime;
        private AudioSource audioSource;

        public void SetupCloud(CosmicStorm storm) {
            this.storm = storm;
            chargeTime = lightningSound.length;
            audioSource = GetComponent<AudioSource>();
        }


        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag == "Player") {
                storm.LightningStrikesPending++;
                Destroy(gameObject);
            }
        }

        IEnumerator LightningStrike(SailsActivatable sail, float yScale) {
            yield return new WaitForSeconds(chargeTime);
            GameObject lightning = Instantiate(lightningPrefab, transform.position, Quaternion.identity);
            audioSource.Stop();
            audioSource.clip = lightningSound;
            audioSource.Play();
            lightning.transform.localScale = new Vector3(lightning.transform.localScale.x, yScale, lightning.transform.localScale.z);
        }

        bool CheckLightningStrike() {
            float velocity = GetComponent<Rigidbody2D>().velocity.x;
            float xOffset = velocity * chargeTime;
            Vector3 targetPosition = new Vector3(transform.position.x + xOffset, transform.position.y, transform.position.z);

            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Sails"));
            Physics2D.Raycast(targetPosition, Vector2.down, filter, hits, lightningHeightBase);
            Debug.DrawRay(targetPosition, Vector2.down * lightningHeightBase, Color.red, 2.5f);
            if (hits.Count > 0) {
                int hitIndex = -1;
                for (int i = 0; i < hits.Count; i++) {
                    if (hits[i].collider?.gameObject?.GetComponent<SailsActivatable>() != null) {
                        hitIndex = i;
                        break;
                    }
                }
                if (hitIndex == -1) return false;
                float distance = Vector2.Distance(targetPosition, hits[hitIndex].point);
                float requiredScale = distance / lightningHeightBase;
                audioSource.clip = chargeSound;
                audioSource.Play();
                StartCoroutine(LightningStrike(hits[hitIndex].collider.gameObject.GetComponent<SailsActivatable>(), requiredScale));
                return true;
            } else {
                return false;
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                Destroy(this.gameObject);
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (storm == null) return;
            if (Random.Range(0, 100) < attemptLightningChance && storm.LightningStrikesPending > 0) {
                Debug.Log("Checking lightning strike");
                storm.LightningStrikesPending--;
                if (!CheckLightningStrike()) {
                    storm.LightningStrikesPending++;
                }
            }
        }
    }
}