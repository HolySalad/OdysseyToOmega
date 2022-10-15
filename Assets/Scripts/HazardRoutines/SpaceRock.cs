using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

namespace SpaceBoat.Hazards {
    public class SpaceRock : MonoBehaviour
    {
        // Create a rock from the prefab with the necessary.
    
        private Vector2 velocity;
        private float height;
        private float scale;


        public void SetupRock(float speed, float angle, float scale, float spawnHeight, float launchTime) {
            // send a rock flying at the player
            this.velocity = new Vector2(-speed * Mathf.Cos(Mathf.Deg2Rad *angle), speed * Mathf.Sin(Mathf.Deg2Rad * angle));
            this.height = spawnHeight;
            this.scale = scale;
            float launchtime = launchTime - Time.timeSinceLevelLoad;
            Debug.Log("Launching a rock at " + launchtime + " with velocity " + velocity + " and height " + height + " and scale " + scale);
            StartCoroutine(LaunchAfterTime(launchtime));
        }

        void OnCollisionEnter2D(Collision2D collision) {
            if (collision.otherCollider.gameObject.layer == LayerMask.NameToLayer("PlayerCharacter")) {
                Player.PlayerLogic player = collision.otherCollider.gameObject.GetComponent<Player.PlayerLogic>();
                Debug.Log("Rock hit player");
                player.PlayerTakesDamage(1);

            } else if (collision.otherCollider.gameObject.layer == LayerMask.NameToLayer("Ground")){
                Destroy(this);
            }
        }

        IEnumerator LaunchAfterTime(float time) {
            yield return new WaitForSeconds(time);// Wait for one second
            transform.localScale = new Vector3(scale, scale, 1);
            transform.position = new Vector3(transform.position.x, height, transform.position.z);
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = velocity;
        }
    }
}