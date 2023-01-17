using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {    
    public class CameraController : MonoBehaviour
    {
        private Collider2D cameraXZone;
        private Player player; 
        private Camera cameraComponent;

        [SerializeField] private Transform shipViewTarget;
        [SerializeField] private float shipViewSize = 34f;
        [SerializeField] private float cameraSizeShiftTime = 1f;
        [SerializeField] private float cameraXYShiftTime = 1f;
        [SerializeField] private float shipViewShiftTime = 1.5f;

        [SerializeField] private float cameraXMax = 100f;
        [SerializeField] private float cameraXMin = -100f;
        [SerializeField] private float shipViewCameraXMax = 100f;
        [SerializeField] private float shipViewCameraXMin = -100f;

        private bool cameraIsAdjustingY = false;
        private bool cameraIsAdjustingX = false;
        private bool wasGroundedYetDuringYTransition = false;
        private bool cameraIsAdjustingSize = false;
        private bool inShipView = false;
        private bool shipViewForced = false;

        private float targetCameraY = 0f;
        private float previousCameraY = 0f;
        private float targetCameraSize = 0f;
        private float previousCameraSize = 0f;
        private float previousCameraX = 0f;
        private float cameraSizeTransitionBeganTime = 0f;
        private float cameraXYTransitionBeganTime = 0f;
        private float shipViewTransitionBeganTime = 0f;

        void Start() {
            cameraXZone = GetComponent<Collider2D>();
            player = GameModel.Instance.player;
            cameraComponent = GetComponent<Camera>();
       
            AdjustCameraX();
            CheckCameraZones();
            cameraComponent.orthographicSize = targetCameraSize;
            transform.position = new Vector3(transform.position.x, targetCameraY, transform.position.z);
            previousCameraSize = cameraComponent.orthographicSize;
            previousCameraY = transform.position.y;
            cameraIsAdjustingY = false;
            cameraIsAdjustingSize = false;
        }

        void CheckCameraZones() {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            List<Collider2D> cameraZones = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("CameraZones"));
            filter.useTriggers = true;
            playerCollider.OverlapCollider(filter, cameraZones);

            float cameraPriority = -1f;
            float newTargetCameraY = targetCameraY;
            float newTargetCameraSize = targetCameraSize;
            string cameraZoneName = "";
            //Debug.Log("Checking " + cameraZones.Count + " camera zones");
            foreach (Collider2D cameraZone in cameraZones) {
                CameraZoneController cameraZoneController = cameraZone.GetComponent<CameraZoneController>();
                if (cameraZoneController != null) {
                    //Debug.Log("Player is overlapping camera zone " + cameraZoneController.gameObject.name + " with priority " + cameraZoneController.priority + ".");
                    if (cameraZoneController.priority > cameraPriority) {
                        cameraPriority = cameraZoneController.priority;
                        cameraZoneName = cameraZoneController.gameObject.name;
                        if (cameraZoneController.camHeight != newTargetCameraY ) {
                           
                            newTargetCameraY = cameraZoneController.camHeight;
                        }
                        if (cameraZoneController.orthographicSize != newTargetCameraSize) {
                           
                            newTargetCameraSize = cameraZoneController.orthographicSize;
                        }
                    }
                }
            }
            if (newTargetCameraY != targetCameraY) {
                cameraIsAdjustingY = true;
                targetCameraY = newTargetCameraY;
                wasGroundedYetDuringYTransition = false;
                Debug.Log("Adjusting camera Y to " + targetCameraY + " for " + cameraZoneName);
            }
            if (newTargetCameraSize != targetCameraSize) {
                cameraIsAdjustingSize = true;
                targetCameraSize = newTargetCameraSize;
                cameraSizeTransitionBeganTime = Time.time;
             Debug.Log("Adjusting camera size to " + targetCameraSize + " for " + cameraZoneName);
            }
        }


        void AdjustCameraX() {
            float targetPosition = Mathf.Clamp(player.transform.position.x + player.playerCameraXFocusOffset, cameraXMin, cameraXMax);
            if (!cameraIsAdjustingX) {
                float currentPosition = transform.position.x;
                if (inShipView) {
                    targetPosition = Mathf.Clamp(player.transform.position.x + player.playerCameraXFocusOffset, shipViewCameraXMin, shipViewCameraXMax);
                }
                transform.position = new Vector3(targetPosition, transform.position.y, transform.position.z);
                return;
            } else if (inShipView) {
                return;
            }
            float transitionPercentTime = Mathf.Min((Time.time - cameraXYTransitionBeganTime) / cameraSizeShiftTime, 1f);
            float newPosition = previousCameraX + ((targetPosition-previousCameraX) * transitionPercentTime);
            transform.position = new Vector3(newPosition, transform.position.y, transform.position.z);
            if (newPosition == targetPosition) {
                cameraIsAdjustingX = false;
            }
        }

        void AdjustCameraY() {
            float currentPosition = transform.position.y;
            //only move camera up if player is grounded
            if (!wasGroundedYetDuringYTransition && currentPosition < targetCameraY && !player.GetIsGrounded(false)) {
                return;
            } else if (!wasGroundedYetDuringYTransition) {
                wasGroundedYetDuringYTransition = true;
                cameraXYTransitionBeganTime = Time.time;
            }
            float transitionPercentTime = Mathf.Min((Time.time - cameraXYTransitionBeganTime) / cameraXYShiftTime, 1f);
            Debug.Log("Camera Y transition percent time: " + transitionPercentTime);
            float newPosition = previousCameraY + ((targetCameraY-previousCameraY) * transitionPercentTime);
            transform.position = new Vector3(transform.position.x, newPosition, transform.position.z);
            if (newPosition == targetCameraY) {
                cameraIsAdjustingY = false;
                previousCameraY = targetCameraY;
            }
        }

        void AdjustCameraSize() {
            float transitionPercentTime = Mathf.Min((Time.time - cameraSizeTransitionBeganTime) / cameraSizeShiftTime, 1f);
            Debug.Log("Camera size transition percent time: " + transitionPercentTime);
            float newSize = previousCameraSize + ((targetCameraSize-previousCameraSize) * transitionPercentTime);
            cameraComponent.orthographicSize = newSize;
            if (newSize == targetCameraSize) {
                Debug.Log("Camera size transition which began at " +cameraSizeTransitionBeganTime+" complete at "+Time.time);
                cameraIsAdjustingSize = false;
                previousCameraSize = targetCameraSize;
            }
        }


        void StartShipView() {
            shipViewTransitionBeganTime = Time.time;
            targetCameraSize = shipViewSize;
            targetCameraY = shipViewTarget.position.y;
        }

        void AdjustShipView() {
            float transitionPercentTime = Mathf.Min((Time.time - shipViewTransitionBeganTime) / shipViewShiftTime, 1f);
            float newPositionY = previousCameraY + ((targetCameraY-previousCameraY) * transitionPercentTime);
            float currentCameraMaxX = cameraXMax + ((shipViewCameraXMax - cameraXMax) * transitionPercentTime);
            float currentCameraMinX = cameraXMin + ((shipViewCameraXMin - cameraXMin) * transitionPercentTime);
            float targetCameraX = Mathf.Clamp(player.transform.position.x + player.playerCameraXFocusOffset, currentCameraMinX, currentCameraMaxX);
            float newPositionX = previousCameraX + ((targetCameraX-previousCameraX) * transitionPercentTime);
            float newSize = previousCameraSize + ((targetCameraSize-previousCameraSize) * transitionPercentTime);
            cameraComponent.orthographicSize = newSize;
            transform.position = new Vector3(newPositionX, newPositionY, transform.position.z);
            if (newPositionY == targetCameraY && newSize == targetCameraSize) {
                previousCameraY = targetCameraY;
                previousCameraSize = targetCameraSize;
                cameraIsAdjustingX = false;
            }
        }


        void Update() {
            bool shipViewHeld = Input.GetKey(KeyCode.C);
            bool wasInShipView = inShipView;
            inShipView = shipViewHeld || shipViewForced;

            if (!inShipView && wasInShipView) {
                previousCameraY = transform.position.y;
                previousCameraSize = cameraComponent.orthographicSize;
                previousCameraX = transform.position.x;
                cameraIsAdjustingX = true;
            } else if (inShipView && !wasInShipView) {
                previousCameraY = transform.position.y;
                previousCameraSize = cameraComponent.orthographicSize;
                previousCameraX = transform.position.x;
                StartShipView();
            }
            if (!inShipView) {
                CheckCameraZones();           
                if (cameraIsAdjustingY) AdjustCameraY();
                if (cameraIsAdjustingSize) AdjustCameraSize();
            } else {
                AdjustShipView();
            }
            
            AdjustCameraX();
 
        }
    }
}