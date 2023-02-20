using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship.Buildables.BuildableExtras {
    public class BuildSystemPlacementMarker : MonoBehaviour
    {
        [SerializeField] private float movementDistancePerSecond = 4f;
        [SerializeField] private float verticalMovementCooldown = 0.3f;
        
        private BuildSystemTrack currentTrack;
        private SpriteRenderer spriteRenderer;
        private IBuildable buildable;
        private float vertMoveTimer = 0f;

        IEnumerator PulseColor() {
            while (true) {
                while (spriteRenderer.color.a <1) {
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a + 0.05f);
                    yield return new WaitForSeconds(0.05f);
                }
                while (spriteRenderer.color.a > 0.55) {
                    spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a - 0.05f);
                    yield return new WaitForSeconds(0.05f);
                }
            }
        }

        void Start() {
            buildable = GetComponentInParent<IBuildable>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            Sprite sprite = buildable.BuildablePrefab.GetComponent<SpriteRenderer>().sprite;
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = Color.green;
            SpriteRenderer spriteUnderlay = transform.Find("SpriteUnderlay").GetComponent<SpriteRenderer>();
            spriteUnderlay.sprite = sprite;
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("BuildableSystem"));
            filter.useTriggers = true;
            Collider2D[] colliders = new Collider2D[2];
            int numColliders = Physics2D.OverlapPoint(transform.position, filter, colliders);
            if (numColliders > 0) {
                foreach (Collider2D collider in colliders) {
                    BuildSystemTrack trackCollider = collider.GetComponent<BuildSystemTrack>();
                    if (trackCollider != null) {
                        currentTrack = trackCollider;
                        return;
                    }
                }
            } else {
                Debug.LogError("BuildSystemPlacementMarker could not find a BuildSystemTrack to attach to.");
            }
            StartCoroutine(PulseColor());
        }

        void Update() {
            //placement
            if (CthulkInput.ActivateKeyDown()) {
                buildable.Build(transform.position);
                StopAllCoroutines();
                UI.UIManager.Instance.ExitBuildMode();
                return;
            }
            //vertical movement
            float VertMovement = CthulkInput.cameraVerticalLook();
            vertMoveTimer -= Time.unscaledDeltaTime;
            if (vertMoveTimer <= 0 && currentTrack.CanMoveVertically(VertMovement)) {
                (Vector2 vertMoveDest, BuildSystemTrack newTrack) = currentTrack.GetMoveDestination(VertMovement, transform);
                transform.position = vertMoveDest;
                currentTrack = newTrack;
                vertMoveTimer = verticalMovementCooldown;
                return;
            }

            //horizontal movement
            float movement = CthulkInput.HorizontalInput();
            if (movement == 0) {
                return;
            }
            
            float moveDist = movement * movementDistancePerSecond * Time.unscaledDeltaTime;
            Debug.Log("Buildable moving" + moveDist + " units");
            Vector2 moveDest = new Vector2(transform.position.x + moveDist, transform.position.y);
            //raycast and check collision with BuildableSystem layer.
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("BuildableSystem"));
            filter.useTriggers = true;
            List<RaycastHit2D> hits = new List<RaycastHit2D>();
            int numHits = Physics2D.Raycast(transform.position, Vector2.right * movement, filter, hits, moveDist);
            if (numHits > 0) {
                hits.Sort((a, b) => a.distance.CompareTo(b.distance));
                foreach (RaycastHit2D hit in hits) {
                    if (hit.collider == null) {
                        continue;
                    }
                    //if there is contact with a LRPortal, check if the portal is the correct type for the movement.
                    //if it is, move the buildable to the other portal.
                    //otherwise, do nothing.
                    BuildSystemLRPortal portalCollider = hit.collider.GetComponent<BuildSystemLRPortal>();
                    if (portalCollider != null) {
                        Debug.Log("Buildable hit LRPortal");
                        if (portalCollider.CheckTeleport(gameObject, movement < 0)) {
                            Debug.Log("Buildable teleporting");
                            return;
                        }
                    }
                }
            }

            Collider2D[] colliders = new Collider2D[2];
            int numColliders = Physics2D.OverlapPoint(moveDest, filter, colliders);
            if (numColliders > 0) {
                foreach (Collider2D collider in colliders) {
                    if (collider == null) {
                        continue;
                    }
                    
                    //if there is no contact with a LRPortal, check if there is contact with a BuildSystemTrack.
                    //if there is, move the buildable along the track.
                    //otherwise, do nothing.
                    BuildSystemTrack trackCollider = collider.GetComponent<BuildSystemTrack>();
                    if (trackCollider != null) {
                        Debug.Log("Buildable hit BuildSystemTrack");
                        transform.position = moveDest;
                        currentTrack = trackCollider;
                        return;
                    }
                }
            }
            Debug.Log("No Track Collider");



        }
    }
}