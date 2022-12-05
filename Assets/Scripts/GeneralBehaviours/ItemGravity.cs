using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat {
    public class ItemGravity : MonoBehaviour
    {
        private Collider2D itemCollider;

        public void Start() {
            itemCollider = GetComponent<Collider2D>();
        }


        public void Update() {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.layerMask = LayerMask.GetMask("Ground");
            int numHits = itemCollider.Cast(Vector2.down, filter, hits, 0.1f);
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") && !hit.collider.gameObject.CompareTag("Platforms")) {
                    return;
                }
            }
            
            transform.position += Vector3.down * 0.1f;
        }
    }
}