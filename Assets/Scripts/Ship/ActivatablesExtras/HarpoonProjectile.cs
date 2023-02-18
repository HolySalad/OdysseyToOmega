using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship 
{    public class HarpoonProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float impactTime = 0.1f;
        [SerializeField] private float reelInDelay = 0.5f;
        [SerializeField] private float endReelDistance = 10f;

        private Vector3 directionFired;
        private Vector3 reelLocation;
        private Rigidbody2D rbHit;
        private Rigidbody2D rbHarpoon;

        public HarpoonGunActivatable harpoonGun;

        private Vector3 RespawnLocation = new Vector3(-2.12f, -8.40f, 0f);

        public void Awake() {
            rbHarpoon = GetComponent<Rigidbody2D>();
        }

        public void Fire(Vector3 direction) {
            reelLocation = transform.position;
            Debug.DrawRay(transform.position, direction*100, Color.red, 5f);
            rbHarpoon.velocity = direction * speed;
            directionFired = direction;
        }



        // reel the harpoon in towards the ship.
        // when 
        IEnumerator ReelInHarpoon() {
            SoundManager.Instance.Play("HarpoonHit");
            Debug.Log("Harpoon impacting!");
            // share velocity on impact
            Vector3 newVelocity = rbHit.velocity + rbHarpoon.velocity;
            rbHit.velocity = newVelocity;
            rbHarpoon.velocity = newVelocity;
            yield return new WaitForSeconds(impactTime);
            newVelocity = newVelocity/4;
            rbHit.velocity = newVelocity;
            rbHarpoon.velocity = newVelocity;
            yield return new WaitForSeconds(reelInDelay-impactTime);
            Debug.Log("Harpoon reeling in!");
            SoundManager.Instance.Play("PullHarpoon");
            newVelocity = newVelocity + (directionFired * -speed/2);
            rbHarpoon.velocity = newVelocity;
            rbHit.velocity = newVelocity;
            float dist = Vector3.Distance(transform.position, reelLocation);
            Debug.Log("Reel distance: " + dist);
            while (Mathf.Abs(dist) > endReelDistance) {
                yield return new WaitForEndOfFrame();
                dist = Vector3.Distance(transform.position, reelLocation);
            }
                            
            Debug.Log("Harpoon detatching!");
            Vector3 pullVector = Vector3.Cross(reelLocation - transform.position, GameModel.Instance.cometDeckTarget.transform.position - transform.position);
            harpoonGun.LoadHarpoon();
            ///rbHarpoon.velocity = pullVector.normalized * speed;
            //Destroy(gameObject); 
            //TransformBackIntoItem();
            
        }

        IEnumerator RespawnHarpoon() {
            yield return new WaitForSeconds(5f);
            harpoonGun.LoadHarpoon();
        }

        public void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds")) {
                Debug.Log("Harpoon hit map bounds");
                StartCoroutine(RespawnHarpoon());
            } else  if (collision.gameObject.layer == LayerMask.NameToLayer("PhysicalHazards")) {
                Destructable destructable = collision.gameObject.GetComponent<Destructable>();
                if (destructable != null) {
                    Debug.Log("Harpoon hit a physical hazard with a destruction behaviour");
                    destructable.Destruct();
                } else {
                    Debug.Log("Harpoon hit a physical hazard without a destruction behaviour");
                    Destroy(collision.gameObject);
                }
            } else if (collision.gameObject.layer == LayerMask.NameToLayer("Harpoonables")) {
                Debug.Log("Harpoon hit a harpoonable");
                rbHit = collision.gameObject.GetComponent<Rigidbody2D>();
                Rewards.RewardComet comet = collision.gameObject.GetComponent<Rewards.RewardComet>();
                if (comet != null) {
                    comet.ShatterComet();
                }
            }
        }


    }
}