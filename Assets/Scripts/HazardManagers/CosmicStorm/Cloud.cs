using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship.Activatables;
using UnityEngine.Rendering.Universal;

namespace SpaceBoat.HazardManagers.CosmicStormSubclasses {    
    public class Cloud : MonoBehaviour
    {

        [SerializeField] private bool isTestCloud = false;        
        [SerializeField] private List<Sprite> cloudSprites;
        [SerializeField] private float lightningHeightBase = 17f;
        [SerializeField] private GameObject lightningPrefab;
        [SerializeField] private AudioClip lightningSound;
        [SerializeField] private AudioClip chargeSound;
        [SerializeField] private Light2D lightSource;
        [SerializeField] private GameObject chargeupAnimation;
        [SerializeField] private float lightChargeupValue = 7f;

        private CosmicStorm storm;
        private float chargeTime;
        private AudioSource audioSource;
        private bool isCharging = false;
        public bool isStriking = false;

        public void SetupCloud(CosmicStorm storm, int order) {
            this.storm = storm;
            chargeTime = lightningSound.length;
            audioSource = GetComponent<AudioSource>();

            int spriteIndex = Random.Range(0, cloudSprites.Count);
            GetComponent<SpriteRenderer>().sprite = cloudSprites[spriteIndex];
            GetComponent<SpriteRenderer>().sortingOrder = order;
            chargeupAnimation.GetComponent<SpriteRenderer>().sortingOrder = order;
        }


        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.tag == "Player" && isCharging && other.gameObject.GetComponent<Player>() != null) {
                other.gameObject.GetComponent<Player>().TakeDamage();
            }
        }

        IEnumerator LightningStrike(SailsActivatable sail, float distance, float additionalDelay = 0f) {
            Debug.DrawRay(transform.position, Vector2.down * distance, Color.red, chargeTime+additionalDelay);
            yield return new WaitForSeconds(additionalDelay);
            isCharging = true;
            lightSource.enabled = true;
            lightSource.intensity = 0f;
            audioSource.clip = chargeSound;
            chargeupAnimation.SetActive(true);
            audioSource.Play();
            float chargeTimer = 0f;
            while (chargeTimer < chargeTime) {
                chargeTimer += Time.deltaTime;
                lightSource.intensity = Mathf.Lerp(0, lightChargeupValue, chargeTimer/chargeTime);
                yield return null;
            }
            float yScale = distance / lightningHeightBase;
            float lightningPosition = transform.position.y - distance/2;
            GameObject lightning = Instantiate(lightningPrefab, new Vector3(transform.position.x, lightningPosition, 0), Quaternion.identity);
            audioSource.Stop();
            audioSource.clip = lightningSound;
            audioSource.Play();

            lightning.transform.localScale = new Vector3(lightning.transform.localScale.x, yScale, lightning.transform.localScale.z);
            sail.Break();
            yield return new WaitForSeconds(0.5f);
            lightSource.enabled = false;
            chargeupAnimation.SetActive(false);
            isCharging = false;
            Destroy(lightning);
        }

        public GameObject CheckLightningStrike(Dictionary<GameObject, bool> targets) {
            float velocity = GetComponent<Rigidbody2D>().velocity.x;
            float xOffset = velocity * chargeTime;
            Vector3 targetPosition = new Vector3(transform.position.x + xOffset, transform.position.y, 0);

            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.useTriggers = true;
            filter.SetLayerMask(LayerMask.GetMask("Sails"));
            Physics2D.Raycast(targetPosition, Vector2.down, filter, hits, lightningHeightBase*5f);
            Debug.DrawRay(targetPosition, Vector2.down * lightningHeightBase*5f, Color.green, Time.deltaTime);
            if (hits.Count > 0) {
                int hitIndex = -1;
                for (int i = 0; i < hits.Count; i++) {
                    if (hits[i].collider != null && targets.ContainsKey(hits[i].collider.gameObject)) {
                        hitIndex = i;
                        break;
                    }
                }
                if (hitIndex == -1) return null;
                isStriking = true;
                RaycastHit2D hit = hits[hitIndex];
                SailsActivatable targetSail = hit.collider.gameObject.GetComponent<SailsActivatable>();
                targetSail.TargetSail();
                Vector3 adjustmentVector = new Vector2(targetSail.hazardTarget.position.x, targetSail.hazardTarget.position.y) - hit.point;
                float distance = Vector2.Distance(targetPosition + adjustmentVector, targetSail.hazardTarget.position);
                Debug.Log("Distance between contact point and transform center: " + Mathf.Abs(hit.transform.position.x - hit.point.x));
                float additionalDelay = hit.point.x - targetSail.hazardTarget.position.x / velocity;
                StartCoroutine(LightningStrike(targetSail, distance, additionalDelay));
                return targetSail.gameObject;
            } else {
                //Debug.Log("Lightning check did not hit anything");
                return null;
            }
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                Destroy(this.gameObject);
            }
        }

        IEnumerator TestCloud() {
            yield return new WaitForSeconds(1f);
            while (true) {
                int spriteIndex = Random.Range(0, cloudSprites.Count);
                GetComponent<SpriteRenderer>().sprite = cloudSprites[spriteIndex];
                yield return new WaitForSeconds(1f);
                GameObject targetSail = GameModel.Instance.shipSails[Random.Range(0, GameModel.Instance.shipSails.Count)];
                SailsActivatable targetSailScript = targetSail.GetComponent<SailsActivatable>();
                float distance = Vector2.Distance(new Vector2(targetSailScript.hazardTarget.position.x, transform.position.y), targetSailScript.hazardTarget.position);
                StartCoroutine(LightningStrike(targetSailScript, distance));
                yield return new WaitForSeconds(chargeTime + 0.5f);
            }
        }

        void Start() {
            chargeupAnimation.GetComponent<Animator>().speed = 0.65f;
            if (isTestCloud) {
                chargeTime = lightningSound.length;
                audioSource = GetComponent<AudioSource>();
                StartCoroutine(TestCloud());
            }
        }

        
    }
}