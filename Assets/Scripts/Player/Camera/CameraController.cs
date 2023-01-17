using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {    
    public class CameraController : MonoBehaviour
    {
        private Collider2D cameraXZone;
        private Player player; 
        private Camera cameraComponent;

        [SerializeField] private float cameraSizeShiftTime = 1f;
        [SerializeField] private float cameraYShiftSpeed = 1f;
        [SerializeField] private float cameraXShiftSpeed = 1f;

        private bool cameraIsAdjustingX = true;
        private bool cameraIsAdjustingY = false;
        private bool wasGroundedYetDuringYTransition = false;
        private bool cameraIsAdjustingSize = false;

        private float targetCameraY = 0f;
        private float targetCameraSize = 0f;
        private float previousCameraSize = 0f;
        private float cameraSizeTransitionBeganTime = 0f;

        void Start() {
            cameraXZone = GetComponent<Collider2D>();
            player = GameModel.Instance.player;
            cameraComponent = GetComponent<Camera>();
            previousCameraSize = cameraComponent.orthographicSize;
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
                wasGroundedYetDuringYTransition = false;
                targetCameraY = newTargetCameraY;
                Debug.Log("Adjusting camera Y to " + targetCameraY + " for " + cameraZoneName);
            }
            if (newTargetCameraSize != targetCameraSize) {
                cameraIsAdjustingSize = true;
                targetCameraSize = newTargetCameraSize;
                cameraSizeTransitionBeganTime = Time.time;
             Debug.Log("Adjusting camera size to " + targetCameraSize + " for " + cameraZoneName);
            }
        }

        void OnTriggerExit2D() {
            cameraIsAdjustingX = true;
        }

        void AdjustCameraX() {
            float currentPosition = transform.position.x;
            float targetPosition = player.transform.position.x + player.playerCameraXFocusOffset;
            //float newPosition = Mathf.MoveTowards(currentPosition, targetPosition, cameraXShiftSpeed * Time.deltaTime);
            transform.position = new Vector3(targetPosition, transform.position.y, transform.position.z);
            //if (newPosition == targetPosition) {
                //cameraIsAdjustingX = false;
            //}
        }

        void AdjustCameraY() {
            float currentPosition = transform.position.y;
            //only move camera up if player is grounded
            if (!wasGroundedYetDuringYTransition && currentPosition < targetCameraY && !player.GetIsGrounded(false)) {
                return;
            }
            wasGroundedYetDuringYTransition = true;
            float newPosition = Mathf.MoveTowards(currentPosition, targetCameraY, cameraYShiftSpeed * Time.deltaTime);
            transform.position = new Vector3(transform.position.x, newPosition, transform.position.z);
            if (newPosition == targetCameraY) {
                cameraIsAdjustingY = false;
            }
        }

        void AdjustCameraSize() {

            float currentSize = cameraComponent.orthographicSize;
            float transitionPercentTime = Mathf.Min((Time.time - cameraSizeTransitionBeganTime) / cameraSizeShiftTime, 1f);
            Debug.Log("Camera size transition percent time: " + transitionPercentTime);
            float newSize = previousCameraSize + ((targetCameraSize-previousCameraSize) * transitionPercentTime);
            cameraComponent.orthographicSize = newSize;
            if (newSize == targetCameraSize) {
                Debug.Log("Camera size transition which began at " +cameraSizeTransitionBeganTime+" complete at "+Time.time);
                cameraIsAdjustingSize = false;
            }
        }


        void Update() {
            CheckCameraZones();
            if (cameraIsAdjustingX) AdjustCameraX();
            if (cameraIsAdjustingY) AdjustCameraY();
            if (cameraIsAdjustingSize) AdjustCameraSize();
        }
    }
}