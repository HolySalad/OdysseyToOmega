using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship.Activatables;

namespace SpaceBoat.HazardManagers.BugSwarmSubclasses {
    public class BugBomb : MonoBehaviour
    {
        [SerializeField] private GameObject explosionAnimationObject;

        private GameObject targetSail;
        private bool isExploding = false;
        private Rigidbody2D rb;
        public void SetTargetSail(GameObject targetSail) {
            this.targetSail = targetSail;
            rb = GetComponent<Rigidbody2D>();
        }

        IEnumerator DestroyAfterExplosion() {
            Animator animator = explosionAnimationObject.GetComponent<Animator>();
            bool hasFrozen = false;
            while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8f) {
                if (!hasFrozen && transform.position.y <= targetSail.transform.position.y) {
                    GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
                    hasFrozen = true;
                }
                yield return null;
            }
            Destroy(gameObject);
        }

        void Update() {
            if (!isExploding && targetSail != null) {
                Vector3 targetPosition = targetSail.transform.position;
                Vector3 targetVector = targetPosition - transform.position;
                rb.velocity = targetVector.normalized * 25f;
            }
        }

        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject == targetSail) {
                targetSail.GetComponent<SailsActivatable>().Break();
                explosionAnimationObject.SetActive(true);
                GetComponent<SpriteRenderer>().enabled = false;
                StartCoroutine(DestroyAfterExplosion());
                SoundManager.Instance.Play("BugExplosion");
            }
            if (isExploding && other.gameObject.layer == LayerMask.NameToLayer("PlayerChar") && other.gameObject.TryGetComponent(out Player playerHealth)) {
                playerHealth.TakeDamage();
            }
        }
    }
}