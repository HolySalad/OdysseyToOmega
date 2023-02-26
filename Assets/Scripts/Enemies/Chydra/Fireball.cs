using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers
{
    public class Fireball : MonoBehaviour
    {
        [SerializeField] private Sprite fireballSprite;

        private Vector2 velocity;

        private GameObject target;
     

        public void SetupMeteor(float speed, Vector3 startingPosition, GameObject targetSail, float soundTime)
        {
      
            //define a vector from the starting position to the target sail
            target = targetSail;
            Vector3 targetVector = targetSail.transform.position - startingPosition;
            // figure out the time it will take to get there
            float timeToTarget = Vector3.Distance(startingPosition, targetSail.transform.position) / speed;
            float launchDelay = soundTime - timeToTarget - 0.1f;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = fireballSprite;
            velocity = new Vector2(targetVector.normalized.x * speed, targetVector.normalized.y * speed);
            SoundManager.Instance.Oneshot("MeteorWhoosh_0");
            float angle = (Mathf.Atan2(target.transform.position.y-transform.position.y, target.transform.position.x-transform.position.x) * Mathf.Rad2Deg)-90;
            if (angle < 0)
            {
                angle += 360f;
            }
            Debug.Log(angle);
            Quaternion rotation = Quaternion.Euler(0, 0, angle);
            gameObject.transform.rotation = rotation;
          
            StartCoroutine(FireMeteor(launchDelay));
        }

        public IEnumerator FireMeteor(float timeToTarget)
        {
            Debug.Log("Fireball launch in " + timeToTarget);
            yield return new WaitForSeconds(timeToTarget);
            Debug.Log("Fireball launched");
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = velocity;
            GetComponent<SpriteRenderer>().enabled = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("Fireball OnTriggerEnter2D");
            if (other.gameObject == target)
            {
                Ship.Activatables.SailsActivatable sail = other.gameObject.GetComponent<Ship.Activatables.SailsActivatable>();
                if (!sail.isBroken)
                {
                    sail.Break();
                    Destroy(this.gameObject);
                }
                SoundManager.Instance.Play("MeteorImpact");
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            Debug.Log("fireball hit " + collision.gameObject.name + " Layer mask " + LayerMask.LayerToName(collision.gameObject.layer));
            if (collision.gameObject.layer == LayerMask.NameToLayer("MapBounds"))
            {
                Destroy(this.gameObject);
            }
            else if (collision.gameObject.layer == LayerMask.NameToLayer("PlayerChar"))
            {
                Debug.Log("fireball hit player");
                GameModel.Instance.player.PlayerTakesDamage();
                Destroy(this.gameObject);
            }
            else if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                Destroy(this.gameObject);
            }
        }
    }
}