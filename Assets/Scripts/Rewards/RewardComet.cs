using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Rewards {
    public class RewardComet : MonoBehaviour
    {
        [SerializeField] private bool ShatterOnStart = false;
        [SerializeField] private List<Sprite> cometSprites = new List<Sprite>();
        [SerializeField] private GameObject spriteObject;
        [SerializeField] private GameObject itemPlaceObject;
        [SerializeField] private GameObject destructionAnimationObject;
        [SerializeField] private GameObject bounceWalkway;

        private GameObject itemPrefab;
        private GameObject secondaryItemPrefab;
        private int numSecondaryItems = 0;

        public delegate void CometShatterCallbackDelegate();
        private List<CometShatterCallbackDelegate> cometShatterCallbacks = new List<CometShatterCallbackDelegate>();
        public void AddCometShatterCallback(CometShatterCallbackDelegate callback) {
            cometShatterCallbacks.Add(callback);
        }

        public void SetupComet(float velocity, Vector3 target, GameObject itemPrefab, GameObject secondaryItemPrefab, int numSecondaryItems) {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Vector3 direction = target - transform.position;
            rb.velocity = direction.normalized * velocity;

            SpriteRenderer sr = spriteObject.GetComponent<SpriteRenderer>();
            sr.sprite = cometSprites[Random.Range(0, cometSprites.Count)];
            StartCoroutine(UpdateComet());
            this.itemPrefab = itemPrefab;
            this.secondaryItemPrefab = secondaryItemPrefab;
            this.numSecondaryItems = numSecondaryItems;

            itemPlaceObject.GetComponent<SpriteRenderer>().sprite = itemPrefab.GetComponent<SpriteRenderer>().sprite;
            bounceWalkway.AddComponent<Extras.CometBounceWalkway>();
        }


        public void ShatterComet() {
            spriteObject.SetActive(false);
            itemPlaceObject.SetActive(false);
            GetComponent<Collider2D>().enabled = false;
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
            if (numSecondaryItems > 0) {
                for (int i = 0; i < numSecondaryItems; i++) {
                    Instantiate(secondaryItemPrefab, 
                    new Vector3(transform.position.x + ((1+i)*1* (Random.Range(0, 2) == 0 ? -1 : 1)), transform.position.y + (1+i)*1* (Random.Range(0, 2) == 0 ? -1 : 1), transform.position.z)
                    , Quaternion.identity);
                }
            }
            destructionAnimationObject.SetActive(true);
            GameModel.Instance.sound.Play("CometBreaking");
            foreach (CometShatterCallbackDelegate callback in cometShatterCallbacks) {
                callback();
            }
        }

        IEnumerator UpdateComet() {
            while (destructionAnimationObject.activeSelf == false) {
                spriteObject.transform.Rotate(0, 0, 5);
                yield return new WaitForSeconds(0.01f);
            }
            
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(rb.velocity.x/2, -10);
            Animator anim = destructionAnimationObject.GetComponent<Animator>();
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 0.8) {
                yield return new WaitForEndOfFrame();
            }
            Destroy(gameObject);
            yield break;
        }

        void Start() {
            if (ShatterOnStart) {
                CometManager cometManager = FindObjectOfType<CometManager>();
                itemPrefab = cometManager.moneyPrefab;
                secondaryItemPrefab = cometManager.moneyPrefab;
                numSecondaryItems = 4;
                StartCoroutine(UpdateComet());
                ShatterComet();
            }
        }

    }
}

namespace SpaceBoat.Rewards.Extras {
    public class CometBounceWalkway: MonoBehaviour, Environment.IBouncable
    {
        public bool Bounce(Player player) {
            float playerVelocity = player.gameObject.GetComponent<Rigidbody2D>().velocity.y;
            
            if (playerVelocity < 0) {
                player.ForceJump(false, true, true);
                Destroy(gameObject.transform.parent.gameObject);
                return true;
            }
            return false;
        }
    }

}