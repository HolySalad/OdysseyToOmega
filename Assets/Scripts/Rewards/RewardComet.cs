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

        public void SetupComet(float velocity, Vector3 target) {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            Vector3 direction = target - transform.position;
            rb.velocity = direction.normalized * velocity;

            SpriteRenderer sr = spriteObject.GetComponent<SpriteRenderer>();
            sr.sprite = cometSprites[Random.Range(0, cometSprites.Count)];
        }

        public void ShatterComet() {
            spriteObject.SetActive(false);
            itemPlaceObject.SetActive(false);
            //TODO create item
            destructionAnimationObject.SetActive(true);
            GameModel.Instance.sound.Play("CometBreaking");
        }

    }
}