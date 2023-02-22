using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Environment {
    public class ShipCapToggler : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer shipCap;
        [SerializeField] private bool hideCap = false;
        [SerializeField] private ShipCapToggler otherToggler;

        IEnumerator FadeCapIn() {
            while (shipCap.color.a < 1) {
                shipCap.color = new Color(shipCap.color.r, shipCap.color.g, shipCap.color.b, shipCap.color.a + 0.05f);
                yield return new WaitForSeconds(0.05f);
            }
        }

        IEnumerator FadeCapOut() {
            while (shipCap.color.a > 0) {
                shipCap.color = new Color(shipCap.color.r, shipCap.color.g, shipCap.color.b, shipCap.color.a - 0.05f);
                yield return new WaitForSeconds(0.05f);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag == "Player")
            {
                if (hideCap) {
                    otherToggler.StopAllCoroutines();
                    StartCoroutine(FadeCapOut());
                } else {
                    otherToggler.StopAllCoroutines();
                    StartCoroutine(FadeCapIn());
                }
            }
        }
    }
}