using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.UI {
    public class CameraControls : MonoBehaviour
    {
        [SerializeField] private Transform shipViewTarget;
        [SerializeField] private float shipViewDistance = 28f;
        [SerializeField] private float playerViewDistance = 16f;
        [SerializeField] private float cameraShiftTime = 1f;
        [SerializeField] private float playerViewHeightOffset = 4f;
        [SerializeField] private float cameraVerticalFollowShiftSpeedUpward = 4f; // units per second.
        [SerializeField] private float cameraVerticalFollowShiftSpeedDownward = 6f; // units per second.
        [SerializeField] private float cameraVerticalSafetyDistance = 12f; // units.
        [SerializeField] private float cameraVerticalSafetySpeed = 10f; // units.
        [SerializeField] private float cameraVerticalFollowStartThreshold = 2f;

        [SerializeField] private bool shipViewByDefault = false;

        private Player player;
        public bool isShipView {get; private set;} = false;
        private bool cameraFollowedLastFrame = false;
        private Vector3 lastGroundedPlayerPosition;
        private Camera mainCamera;
        private float transitionTargetEndTime = 0f; 
        private float playerRotationXOffset = 0f;

        [SerializeField]  private float camBoundXMin;
        [SerializeField] private float camBoundXMax;
        [SerializeField] private float camBoundYMin;
        [SerializeField] private float camBoundYMax;

        [SerializeField] private float disableShipCamBelowY = -10f;
        private bool restoreCamera = false;

        void Start() {
            player = GameModel.Instance.player;
            isShipView = shipViewByDefault;
            mainCamera = GetComponent<Camera>();
            if (isShipView) {
                mainCamera.orthographicSize = shipViewDistance;
                transform.position = shipViewTarget.position;
            } else {
                Vector3 playerPos = GameModel.Instance.player.transform.position;
                Vector3 targetPos =  new Vector3(Mathf.Clamp(playerPos.x + playerRotationXOffset, camBoundXMin, camBoundXMax), Mathf.Clamp(playerPos.y + playerViewHeightOffset, camBoundYMin, camBoundYMax), -10);
                transform.position = targetPos;
                mainCamera.orthographicSize = playerViewDistance;
            }
        }


        public void ToggleShipView() {
            if (player.gameObject.transform.position.y < disableShipCamBelowY) {
                return;
            }
            isShipView = !isShipView;
            transitionTargetEndTime = Time.time + cameraShiftTime;
        }

        public void ToggleShipView(bool forceSetting) {
            isShipView = forceSetting;
            transitionTargetEndTime = Time.time + cameraShiftTime;
        }

        public void SetPlayerFocusXOffset(float offset) {
            playerRotationXOffset = offset;
            /*bool isTransitioning = Time.time < transitionTargetEndTime;
            if (!isTransitioning) {
                Vector3 playerPos = GameModel.Instance.player.transform.position;
                transform.position =  new Vector3(playerPos.x + playerRotationXOffset, playerPos.y + playerViewHeightOffset, -10);
            }*/

        }

        private void ShipViewUpdate() {
            bool isTransitioning = Time.time < transitionTargetEndTime;
            if (isTransitioning) {
                float interpolationRatio =  1- (transitionTargetEndTime - Time.time) / cameraShiftTime;
                Debug.Log("Transitioning to ship view: Interpolation Ratio: " + interpolationRatio);
                transform.position = Vector3.Lerp(transform.position, shipViewTarget.position, interpolationRatio);
                mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, shipViewDistance, interpolationRatio);
            }
        }

        private void PlayerViewUpdate() {
            bool isTransitioning = Time.time < transitionTargetEndTime;
            Vector3 playerPos = player.transform.position;
            if (player.GetIsGrounded(true) && !player.isSlipping) {
                lastGroundedPlayerPosition = playerPos;
            } else if (playerPos.y > lastGroundedPlayerPosition.y) {
                // Don't follow the player upwards when they are jumping.
                playerPos.y = lastGroundedPlayerPosition.y;
            }
            Vector3 targetPos = new Vector3(playerPos.x + playerRotationXOffset, playerPos.y + playerViewHeightOffset, -10);
            if (!isTransitioning) {

                if (Mathf.Abs(targetPos.y  - transform.position.y ) < cameraVerticalFollowStartThreshold && !cameraFollowedLastFrame &&
                transform.position.y != camBoundYMin) {
                    // Don't follow extremely small vertical distances unless the camera is already moving or the camera was touching the floor.
                    targetPos.y = transform.position.y;
                } else if (Mathf.Abs(targetPos.y  - transform.position.y )> cameraVerticalSafetyDistance) {
                    if (targetPos.y > transform.position.y) {
                        targetPos.y = Mathf.Max(transform.position.y + (cameraVerticalSafetySpeed * Time.deltaTime), transform.position.y);
                    } else if (targetPos.y < transform.position.y) {
                        targetPos.y = Mathf.Min(transform.position.y - (cameraVerticalSafetySpeed * Time.deltaTime), transform.position.y);
                    }
                    Debug.Log("Camera safety Distance hit");
                } /*else if (!playerGrounded && targetPos.y > transform.position.y) {
                    // only follow the player downwards when they are jumping, not upwards.
                    targetPos.y = transform.position.y;
                }*/ else if (targetPos.y < transform.position.y) {
                    // limit the follow speed when following downwards.
                    targetPos.y = Mathf.Max(targetPos.y, transform.position.y - (cameraVerticalFollowShiftSpeedDownward * Time.deltaTime));
                } else if (targetPos.y > transform.position.y) {
                    // limit the follow speed when following upwards.
                    targetPos.y = Mathf.Min(targetPos.y, transform.position.y + (cameraVerticalFollowShiftSpeedUpward * Time.deltaTime));
                } 
                targetPos =  new Vector3(Mathf.Clamp(targetPos.x, camBoundXMin, camBoundXMax), Mathf.Clamp(targetPos.y, camBoundYMin, camBoundYMax), -10);
                cameraFollowedLastFrame = targetPos.y != transform.position.y;
                transform.position = targetPos;
            } else {
                float interpolationRatio = 1- (transitionTargetEndTime - Time.time) / cameraShiftTime;
                Debug.Log("Transitioning to player view: Interpolation Ratio: " + interpolationRatio);
                targetPos =  new Vector3(Mathf.Clamp(targetPos.x, camBoundXMin, camBoundXMax), Mathf.Clamp(targetPos.y, camBoundYMin, camBoundYMax), -10);
                transform.position = Vector3.Lerp(transform.position, targetPos, interpolationRatio);
                mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, playerViewDistance, interpolationRatio);
            }
            
        }

        void Update() {
            if (isShipView) {
                //Debug.Log("In ship view, player y is " + player.gameObject.transform.position.y + " and disableShipCamBelowY is " + disableShipCamBelowY);
                if (player.gameObject.transform.position.y < disableShipCamBelowY) {
                    Debug.Log("Disabling ship view because we're below deck");
                    restoreCamera = isShipView;
                    ToggleShipView(false);
                    return;
                }
                ShipViewUpdate();
            } else {
                PlayerViewUpdate();
                if (lastGroundedPlayerPosition.y > disableShipCamBelowY && restoreCamera) {
                    Debug.Log("Enabling ship view because we're above deck");
                    ToggleShipView(true);
                    restoreCamera = false;
                    return;
                }
            }
        }
    }
}