using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat {
    public class ItemGravity : MonoBehaviour
    {
        private Collider2D itemCollider;
        private float acceleration = 0.1f;
        private float maxSpeed = 0.5f;
        private float currentSpeed = 0f;

        public void Start() {
            itemCollider = GetComponent<Collider2D>();
        }


        public void Update() {
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("Ground"));
            int numHits = itemCollider.Cast(Vector2.down, filter, hits, 0.1f);
            foreach (RaycastHit2D hit in hits) {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground") && !hit.collider.gameObject.CompareTag("Platforms")) {
                    currentSpeed = 0f;
                    return;
                }
            }
            currentSpeed = Mathf.Min(currentSpeed + acceleration, maxSpeed);
            
            transform.position += Vector3.down * currentSpeed;
        }
    }
}