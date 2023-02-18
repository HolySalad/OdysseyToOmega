using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Rewards {
    public class RewardComet : MonoBehaviour
    {
        [SerializeField] private List<Sprite> cometSprites = new List<Sprite>();
        [SerializeField] private GameObject spriteObject;
        [SerializeField] private GameObject itemPlaceObject;
        [SerializeField] private GameObject destructionAnimationObject;

        private GameObject itemPrefab;

        public void SetupComet(float velocity, Vector3 target, GameObject itemPrefab) {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Vector3 direction = target - transform.position;
            rb.velocity = direction.normalized * velocity;

            SpriteRenderer sr = spriteObject.GetComponent<SpriteRenderer>();
            sr.sprite = cometSprites[Random.Range(0, cometSprites.Count)];
            StartCoroutine(UpdateComet());
            this.itemPrefab = itemPrefab;

            itemPlaceObject.GetComponent<SpriteRenderer>().sprite = itemPrefab.GetComponent<SpriteRenderer>().sprite;
        }

        public void ShatterComet() {
            spriteObject.SetActive(false);
            itemPlaceObject.SetActive(false);
            Instantiate(itemPrefab, transform.position, Quaternion.identity);
            destructionAnimationObject.SetActive(true);
            GameModel.Instance.sound.Play("CometBreaking");
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

    }
}