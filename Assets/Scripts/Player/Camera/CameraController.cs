using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {    
    public class CameraController : MonoBehaviour
    {
        private Player player; 
        private Camera cameraComponent;

        [SerializeField] private Transform shipViewTarget;
        [SerializeField] private float shipViewSize = 34f;
        [SerializeField] private float cameraShiftTime = 1f;
        [SerializeField] private float shiftTimeReductionProportion = 0.7f;

        [SerializeField] private float cameraXMax = 100f;
        [SerializeField] private float cameraXMin = -100f;
        [SerializeField] private float shipViewCameraXMax = 100f;
        [SerializeField] private float shipViewCameraXMin = -100f;

        private bool cameraInitialized = false;
        private bool inShipView = false;
        private bool shipViewForced = false;
        private float cameraTargetY = 0f;
        private float cameraTargetX = 0f;
        private float cameraTargetSize = 0f;

        private float currentCameraMovementOriginY = 0f;
        private float currentCameraMovementOriginSize = 0f;
        private float currentCameraMovementOriginX = 0f;

        private float cameraMovementTargetEndTime = 0f;
        private float currentTargetTransitionDuration = 0f;
        private bool inShipViewTransition = false;

        void Start() {
            player = GameModel.Instance.player;
            cameraComponent = GetComponent<Camera>();

        }

        (float, float) GetCurrentCameraZoneValues() {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            List<Collider2D> cameraZones = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("CameraZones"));
            filter.useTriggers = true;
            playerCollider.OverlapCollider(filter, cameraZones);

            float newSize = cameraTargetSize;
            float newY = cameraTargetY;
            float priority = -1f;
            foreach (Collider2D cameraZone in cameraZones) {
                CameraZoneController cameraZoneController = cameraZone.GetComponent<CameraZoneController>();
                if (cameraZoneController != null) {
                   if (cameraZoneController.priority > priority) {
                        priority = cameraZoneController.priority;
                        newSize = cameraZoneController.orthographicSize;
                        newY = cameraZoneController.camHeight;
                    }
                }
            }
            return (newSize, newY);
        }

        void SetCameraTargetSizeAndY() {


            float previousSize = cameraTargetSize;
            float previousY = cameraTargetY;
            float newSize = 0f;
            float newY = 0f;

            if (inShipView) {
                newSize = shipViewSize;
                newY = shipViewTarget.position.y;
            } else {
                (newSize, newY) = GetCurrentCameraZoneValues();
            }

            if (!cameraInitialized) {
                cameraTargetSize = newSize;
                cameraTargetY = newY;
                cameraComponent.orthographicSize = newSize;
                transform.position = new Vector3(player.transform.position.x+ player.playerCameraXFocusOffset, newY, transform.position.z);
                cameraInitialized = true;
                return;
            }

            if (newY == previousY && newSize == previousSize) {
                return;
            }
            Debug.Log("Player camera target changes from " + previousY + " to " + newY + " Y and from " + previousSize + " to " + newSize + " Size");
            // don't change the camera to a higher level until the player is grounded.
            if (newY > previousY && !player.GetIsGrounded(false) && !inShipViewTransition) {
                Debug.Log("Player is not grounded, not adjusting camera.");
                return;
            }

            // set the target end time for the camera movement
            //this covers cases where the player moves between two camera zones or modes before the camera has finished moving
            float changeProportion = 0f;
            float diffY = Mathf.Abs(newY - previousY);
            float diffSize = Mathf.Abs(newSize - previousSize);
            float requiredChangeY = Mathf.Abs(newY - transform.position.y);
            float requiredChangeSize = Mathf.Abs(newSize - cameraComponent.orthographicSize);
            if (diffY == 0 && diffSize == 0)
                changeProportion = 0;
            else if (diffY == 0)
                changeProportion = requiredChangeSize/diffSize;
            else if (diffSize == 0)
                changeProportion = requiredChangeY/diffY;
            else
                changeProportion = (requiredChangeY/diffY >= requiredChangeSize/diffSize) ? requiredChangeY/diffY : requiredChangeSize/diffSize;

            float cameraMovementDuration = (cameraShiftTime*(1-shiftTimeReductionProportion)) + (changeProportion * cameraShiftTime * shiftTimeReductionProportion);
            currentTargetTransitionDuration = cameraMovementDuration;
            cameraMovementTargetEndTime = Time.time + cameraMovementDuration;
            Debug.Log("Camera movement duration " + cameraMovementDuration + " target end time: " + cameraMovementTargetEndTime);
            //set movement origins and targets
            currentCameraMovementOriginY = transform.position.y;
            currentCameraMovementOriginSize = cameraComponent.orthographicSize;
            cameraTargetY = newY;
            cameraTargetSize = newSize;
        }

        void SetCameraTargetX() {
            float newXTarget = player.transform.position.x + player.playerCameraXFocusOffset;
            if (inShipView) {
                newXTarget = Mathf.Clamp(newXTarget, shipViewCameraXMin, shipViewCameraXMax);
            } else {
                newXTarget = Mathf.Clamp(newXTarget, cameraXMin, cameraXMax);
            }
            cameraTargetX = newXTarget;
        }

        
        void MoveAndResizeCamera() {


            float percentageMovementComplete = 1 - Mathf.Max((cameraMovementTargetEndTime - Time.time) / currentTargetTransitionDuration, 0);
            if (percentageMovementComplete == 1) {
                inShipViewTransition = false;
                transform.position = new Vector3(cameraTargetX, cameraTargetY, transform.position.z);
                cameraComponent.orthographicSize = cameraTargetSize;
                return;
            }
            Vector3 newCameraPosition = transform.position;
            Debug.Log("Camera is moving, percentage complete: " + percentageMovementComplete);
            if (inShipViewTransition) {
                newCameraPosition.x = currentCameraMovementOriginX + ((cameraTargetX - currentCameraMovementOriginX) * percentageMovementComplete);
            } else {
                newCameraPosition.x = cameraTargetX;
            }
            newCameraPosition.y = currentCameraMovementOriginY + ((cameraTargetY - currentCameraMovementOriginY) * percentageMovementComplete);
            transform.position = newCameraPosition;
            cameraComponent.orthographicSize = currentCameraMovementOriginSize + ((cameraTargetSize - currentCameraMovementOriginSize) * percentageMovementComplete);
        }

        void Update() {
            bool shipViewHeld = Input.GetKey(KeyCode.C);
            bool wasInShipView = inShipView;
            inShipView = shipViewHeld || shipViewForced;

            bool shipViewStateChanged = (!inShipView && wasInShipView) ||  (inShipView && !wasInShipView);
            if (shipViewStateChanged) {
                currentCameraMovementOriginX = transform.position.x;
                inShipViewTransition = true;
            } 

            SetCameraTargetX();
            SetCameraTargetSizeAndY();
            MoveAndResizeCamera();
        }
    }
}