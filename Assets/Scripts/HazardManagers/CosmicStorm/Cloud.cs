using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship;

namespace SpaceBoat.HazardManagers {    
    public class Cloud : MonoBehaviour
    {
        
        [SerializeField] private List<Sprite> cloudSprites;
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

            int spriteIndex = Random.Range(0, cloudSprites.Count);
            GetComponent<SpriteRenderer>().sprite = cloudSprites[spriteIndex];
        }


        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag == "Player") {
                storm.LightningStrikesPending++;
                Destroy(gameObject);
            }
        }

        IEnumerator LightningStrike(SailsActivatable sail, float distance, float additionalDelay = 0f) {
            yield return new WaitForSeconds(additionalDelay);
            audioSource.clip = chargeSound;
            audioSource.Play();
            yield return new WaitForSeconds(chargeTime);
            float yScale = distance / lightningHeightBase;
            float lightningPosition = transform.position.y - distance/2;
            GameObject lightning = Instantiate(lightningPrefab, new Vector3(transform.position.x, lightningPosition, 0), Quaternion.identity);
            audioSource.Stop();
            audioSource.clip = lightningSound;
            audioSource.Play();

            lightning.transform.localScale = new Vector3(lightning.transform.localScale.x, yScale, lightning.transform.localScale.z);
            sail.Break();
            yield return new WaitForSeconds(0.5f);
            Destroy(lightning);
        }

        bool CheckLightningStrike() {
            float velocity = GetComponent<Rigidbody2D>().velocity.x;
            float xOffset = velocity * chargeTime;
            Vector3 targetPosition = new Vector3(transform.position.x + xOffset, transform.position.y, 0);

            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(LayerMask.GetMask("Sails"));
            Physics2D.Raycast(targetPosition, Vector2.down, filter, hits, lightningHeightBase*5f);
            Debug.DrawRay(targetPosition, Vector2.down * lightningHeightBase*5f, Color.red, 2.5f);
            if (hits.Count > 0) {
                int hitIndex = -1;
                for (int i = 0; i < hits.Count; i++) {
                    if (hits[i].collider?.gameObject?.GetComponent<SailsActivatable>() != null) { 
                        SailsActivatable sail = hits[i].collider.gameObject.GetComponent<SailsActivatable>();
                        if (sail.isBroken == false && sail.isTargetted == false 
                        && hits[i].point.x > sail.hazardTarget.position.x
                        &&  (!sail.IsOnCooldown() || GameModel.Instance.lastSurvivingSailCount <= 1)) {
                            hitIndex = i;
                            break;
                        }
                    }
                }
                if (hitIndex == -1) return false;
                RaycastHit2D hit = hits[hitIndex];
                SailsActivatable targetSail = hit.collider.gameObject.GetComponent<SailsActivatable>();
                targetSail.TargetSail();
                Vector3 adjustmentVector = new Vector2(targetSail.hazardTarget.position.x, targetSail.hazardTarget.position.y) - hit.point;
                float distance = Vector2.Distance(targetPosition + adjustmentVector, targetSail.hazardTarget.position);
                Debug.Log("Distance between contact point and transform center: " + Mathf.Abs(hit.transform.position.x - hit.point.x));
                float additionalDelay = hit.point.x - targetSail.hazardTarget.position.x / velocity;
                StartCoroutine(LightningStrike(targetSail, distance, additionalDelay));
                return true;
            } else {
                //Debug.Log("Lightning check did not hit anything");
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
                //Debug.Log("Checking lightning strike");
                storm.LightningStrikesPending--;
                if (!CheckLightningStrike()) {
                    storm.LightningStrikesPending++;
                }
            }
        }
    }
}