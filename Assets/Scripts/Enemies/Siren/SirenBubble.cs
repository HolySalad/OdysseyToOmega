using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.PlayerSubclasses.PlayerStates;

namespace SpaceBoat.Enemies.Siren {
    public class SirenBubble : MonoBehaviour
    {
        [SerializeField] private float speed = 6f;
        [SerializeField] private float lifeTime = 8f;
        [SerializeField] private float playerCaptureTime = 2.5f;
        [SerializeField] private float playerMaxMoveIncrement = 0.1f;

        private GameObject trappedPlayer;
        private float timeAlive = 0f;
        private bool hasCapturedPlayer = false;
        private float bubbleYOffset = 0f;

        IEnumerator Bubble() {
            while (timeAlive < lifeTime) {
                timeAlive += Time.deltaTime;
                if (trappedPlayer != null) {
                    trappedPlayer.transform.position = new Vector3(
                        Mathf.Clamp(transform.position.x, trappedPlayer.transform.position.x - playerMaxMoveIncrement, trappedPlayer.transform.position.x + playerMaxMoveIncrement),
                        Mathf.Clamp(transform.position.y+bubbleYOffset, trappedPlayer.transform.position.y - playerMaxMoveIncrement, trappedPlayer.transform.position.y + playerMaxMoveIncrement),
                        trappedPlayer.transform.position.z
                    );
                }
                yield return null;
            }
            Destroy(gameObject);
        }

        private void OnTriggerEnter2D(Collider2D collision) {
            if (hasCapturedPlayer) return;
            if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<Player>() != null) {
                Debug.Log("Captured player");
                trappedPlayer = collision.gameObject;
                CapturedState capturedState = trappedPlayer.GetComponent<CapturedState>();
                capturedState.SetReleaseTimer(Mathf.Min(playerCaptureTime, lifeTime - timeAlive));
                capturedState.SetCapturedVelocity(new Vector2(0f, speed));
                hasCapturedPlayer = true;
                trappedPlayer.GetComponent<Player>().ChangeState(PlayerStateName.captured);
                bubbleYOffset = trappedPlayer.transform.position.y - trappedPlayer.GetComponent<Player>().bodyCollider.position.y;
            }
        }

        public void setupBubble() {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0f, speed);
            StartCoroutine(Bubble());
        }
    }
}