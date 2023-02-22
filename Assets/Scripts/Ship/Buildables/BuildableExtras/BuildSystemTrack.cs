using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Ship.Buildables.BuildableExtras {
    public class BuildSystemTrack : MonoBehaviour
    {
        [SerializeField] private BuildSystemTrack higherTrack;
        [SerializeField] private BuildSystemTrack lowerTrack;

        public bool CanMoveVertically(float movement) {
            if (movement == 0) {return false;}
            Debug.Log("Checking if buildable target can move vertically");
            if (movement > 0) {
                return higherTrack != null;
            }
            if (movement < 0) {
                return lowerTrack != null;
            }
            return false;
        }

        public (Vector2, BuildSystemTrack) GetMoveDestination(float movement, Transform currentPos) {
            if (movement == 0) {return (currentPos.position, this);}
            BuildSystemTrack targetTrack = higherTrack;
            if (movement < 0) {
                targetTrack = lowerTrack;
            }
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("BuildableSystem"));
            filter.useTriggers = true;
            List<RaycastHit2D> colliders = new List<RaycastHit2D>();
            Vector2 RaycastStart = new Vector2(currentPos.position.x, this.transform.position.y);
            Vector2 raycastDir = Vector2.right;
            if (currentPos.position.x > this.transform.position.x) {
                raycastDir = Vector2.left;
            }

            int numColliders = Physics2D.Raycast((Vector2)currentPos.position, raycastDir, filter, colliders, 100f);
            if (numColliders > 0) {
                colliders.Sort((a, b) => a.distance.CompareTo(b.distance));
                foreach (RaycastHit2D hit in colliders) {
                    if (hit.collider == null) {
                        continue;
                    }
                    BuildSystemTrack trackCollider = hit.collider.GetComponent<BuildSystemTrack>();
                    if (trackCollider != null) {
                        return (new Vector2(hit.point.x, trackCollider.transform.position.y), this);
                    }
                }
            }
            

            return (currentPos.position, this);
        }
    }
}