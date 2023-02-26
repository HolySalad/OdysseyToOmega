using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {    
    public class CameraController : MonoBehaviour
    {
        private Player player; 
        private Camera cameraComponent;

        public struct ForcedCamera {
            public string name;
            public Transform target;
            public int priority;
            public bool isXPositionOverride;
            public bool isCameraPositionOverride;

            public bool overrideControls;

            public ForcedCamera(string name, Transform target, int priority, bool isXPositionOverride, bool isCameraPositionOverride, bool overrideControls = false) {
                this.name = name;
                this.target = target;
                this.priority = priority;
                this.isXPositionOverride = isXPositionOverride;
                this.isCameraPositionOverride = isCameraPositionOverride;
                this.overrideControls = overrideControls;
            }
        }

        private List<ForcedCamera> shipViewOverrides = new List<ForcedCamera>();

        [SerializeField] private Transform shipViewTarget;
        [SerializeField] private float shipViewSize = 34f;
        [SerializeField] private float cameraShiftTime = 1f;
        [SerializeField] private float playerFallingShiftTimeMultiplier = 0.5f;
        [SerializeField] private float shiftTimeReductionProportion = 0.7f;
        [SerializeField] private float cameraLookTime = 0.5f;
        [SerializeField] private float baseCameraXOffset = 3.5f;
        [SerializeField] private float lookRightCameraXOffset = 5f;
        [SerializeField] private float lookDownCameraYOffset = -4f;
        [SerializeField] private float lookUpCameraYOffset = 3f;

        [SerializeField] private float cameraXMax = 100f;
        [SerializeField] private float cameraXMin = -100f;
        [SerializeField] private float shipViewCameraXMax = 100f;
        [SerializeField] private float shipViewCameraXMin = -100f;
        [SerializeField] private float headlampRotation = 30f;
        [SerializeField] private float headlampRotationSpeed = 20f;

        private bool cameraInitialized = false;
        private bool inShipView = false;
        private bool shipViewHeld = false;
        private bool shipViewForced = false;
        private bool controlsLocked = false;
        private Transform originalShipViewTarget = null;

        private bool hasXPositionOverride = false;
        private float xPositionOverride = 0f;

        private bool cameraBehaviourForced = false;
        private float cameraTargetY = 0f;
        private float cameraTargetX = 0f;
        private float cameraTargetSize = 0f;

        private bool cameraLookRightToggled = false;
        private float currentLookOffsetRight = 0f;
        private float currentLookOffsetDown = 0f;

        private float currentCameraMovementOriginY = 0f;
        private float currentCameraMovementOriginSize = 0f;
        private float currentCameraMovementOriginX = 0f;

        private float cameraMovementTargetEndTime = 0f;
        private float currentTargetTransitionDuration = 0f;
        private bool inShipViewTransition = false;
        private float currentHeadLampTarget = 0;

        void CamLog(string message) {
            Debug.Log("CAMERA: " + message);
        }

        void Start() {
            player = GameModel.Instance.player;
            cameraComponent = GetComponent<Camera>();
            originalShipViewTarget = shipViewTarget;
        }

        (float, float, bool) GetCurrentCameraZoneValues() {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            List<Collider2D> cameraZones = new List<Collider2D>();
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(LayerMask.GetMask("CameraZones"));
            filter.useTriggers = true;
            playerCollider.OverlapCollider(filter, cameraZones);

            float newSize = cameraTargetSize;
            float newY = cameraTargetY;
            float priority = -1f;
            bool supressFastFallingCameraShift = false;
            foreach (Collider2D cameraZone in cameraZones) {
                CameraZoneController cameraZoneController = cameraZone.GetComponent<CameraZoneController>();
                if (cameraZoneController != null) {
                   if (cameraZoneController.priority > priority) {
                        priority = cameraZoneController.priority;
                        newSize = cameraZoneController.orthographicSize;
                        newY = cameraZoneController.camHeight;
                        supressFastFallingCameraShift = cameraZoneController.supressFastFallingCameraShift;
                    }
                }
            }
            return (newSize, newY, supressFastFallingCameraShift);
        }

        void SetCameraTargetSizeAndY() {


            float previousSize = cameraTargetSize;
            float previousY = cameraTargetY;
            float newSize = 0f;
            float newY = 0f;
            bool supressFastFallingCameraShift = false;

            if (inShipView) {
                newSize = shipViewSize;
                newY = shipViewTarget.position.y;
            } else {
                (newSize, newY, supressFastFallingCameraShift) = GetCurrentCameraZoneValues();
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
            CamLog("Player camera target changes from " + previousY + " to " + newY + " Y and from " + previousSize + " to " + newSize + " Size");
            // don't change the camera to a higher level until the player is grounded.
            bool grounded = player.GetIsGrounded(false, true);
            if (newY > previousY && !grounded && !inShipViewTransition) {
                CamLog("Player is not grounded, not adjusting camera upwards.");
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
            if (!supressFastFallingCameraShift && newY < previousY && !grounded && !inShipViewTransition && diffY > newSize/2.2) {
                (bool isJumping, bool fastFall, bool halfJump, bool hitApex) = player.GetJumpStatus();
                CamLog("Player is falling, adjusting camera downwards at gravity terminal velocity.");
                if (!isJumping || (isJumping && hitApex)) {
                    cameraMovementDuration = diffY / player.gravityTerminalVelocity;
                }
            }
            currentTargetTransitionDuration = cameraMovementDuration;
            cameraMovementTargetEndTime = Time.realtimeSinceStartup + cameraMovementDuration;
            CamLog("Camera movement duration " + cameraMovementDuration + " target end time: " + cameraMovementTargetEndTime);
            //set movement origins and targets
            currentCameraMovementOriginY = transform.position.y - currentLookOffsetDown;
            currentCameraMovementOriginSize = cameraComponent.orthographicSize;
            cameraTargetY = newY;
            cameraTargetSize = newSize;
        }

        void SetCameraTargetX() {
            float scaledXOffset = (baseCameraXOffset + currentLookOffsetRight) * (cameraComponent.orthographicSize / 10);
            float newXTarget = player.transform.position.x + scaledXOffset + player.playerCameraXFocusOffset;
            if (hasXPositionOverride) {
                newXTarget = xPositionOverride;
            } else if (inShipView) {
                newXTarget = Mathf.Clamp(newXTarget, shipViewCameraXMin, shipViewCameraXMax);
            } else {
                newXTarget = Mathf.Clamp(newXTarget, cameraXMin, cameraXMax);
            }
            cameraTargetX = newXTarget;
        }

        
        void MoveAndResizeCamera() {
            float percentageMovementComplete = 1 - Mathf.Max((cameraMovementTargetEndTime - Time.realtimeSinceStartup) / currentTargetTransitionDuration, 0);
            if (percentageMovementComplete == 1) {
                inShipViewTransition = false;
                transform.position = new Vector3(cameraTargetX, cameraTargetY + currentLookOffsetDown, transform.position.z);
                cameraComponent.orthographicSize = cameraTargetSize;
                return;
            }
            Vector3 newCameraPosition = transform.position;
            //CamLog("Camera is moving, percentage complete: " + percentageMovementComplete);
            if (inShipViewTransition) {
                newCameraPosition.x = currentCameraMovementOriginX + ((cameraTargetX - currentCameraMovementOriginX) * percentageMovementComplete);
            } else {
                newCameraPosition.x = cameraTargetX;
            }
            newCameraPosition.y = currentCameraMovementOriginY + currentLookOffsetDown + ((cameraTargetY - currentCameraMovementOriginY) * percentageMovementComplete);
            transform.position = newCameraPosition;
            cameraComponent.orthographicSize = currentCameraMovementOriginSize + ((cameraTargetSize - currentCameraMovementOriginSize) * percentageMovementComplete);
        }

        void SetHeadlampState(float verticalInput) {
            GameObject headlamp = player.headCollider.gameObject;
            float existingXRotation = headlamp.transform.rotation.eulerAngles.x;
            if (verticalInput < 0) {
                currentHeadLampTarget = -headlampRotation;
            } else if (verticalInput > 0) {
                currentHeadLampTarget = headlampRotation;
            } else {
                currentHeadLampTarget = 0;
            } 
            float existingZRotation = headlamp.transform.rotation.eulerAngles.z;
            if ((int)existingZRotation > 30) {
                existingZRotation -= 360;
            }
            if ((int)existingZRotation != (int)currentHeadLampTarget) {

                //CamLog("Headlamp rotation is " + existingZRotation + " and target is " + currentHeadLampTarget);
                float neededChange = currentHeadLampTarget - existingZRotation;
                float changeThisFrame = Mathf.Min(headlampRotationSpeed * Time.unscaledDeltaTime, Mathf.Abs(neededChange));
                //CamLog("Needed change " + neededChange +  " Headlamp rotation change this frame: " + changeThisFrame);
                float newRotation = Mathf.Clamp(existingZRotation + (changeThisFrame*Mathf.Sign(neededChange)), -headlampRotation, headlampRotation);
                //CamLog("New headlamp rotation: " + newRotation);
                headlamp.transform.rotation = Quaternion.Euler(existingXRotation, 0, newRotation);
            }
        }


        void Update() {
            if (cameraBehaviourForced) {
                MoveAndResizeCamera();
                return;
            }
            if (CthulkInput.CameraToggleDown()) {
                shipViewHeld = !shipViewHeld;
            }
            float verticalLook = CthulkInput.cameraVerticalLook();
            SetHeadlampState(verticalLook);
            if (verticalLook != 0 && !inShipView && !GameModel.Instance.isPaused) {
                if (verticalLook < 0 && currentLookOffsetDown > lookDownCameraYOffset) {
                    float changePerSecond = lookDownCameraYOffset / cameraLookTime;
                    currentLookOffsetDown = Mathf.Max(currentLookOffsetDown + (changePerSecond * Time.unscaledDeltaTime), lookDownCameraYOffset);
                } else if (verticalLook > 0 && currentLookOffsetDown < lookUpCameraYOffset) {
                    float changePerSecond = lookUpCameraYOffset / cameraLookTime;
                    currentLookOffsetDown = Mathf.Min(currentLookOffsetDown + (changePerSecond * Time.unscaledDeltaTime), lookUpCameraYOffset);
                }
            } else {
                if (currentLookOffsetDown < 0) {
                    float changePerSecond = lookDownCameraYOffset / cameraLookTime;
                    currentLookOffsetDown = Mathf.Min(currentLookOffsetDown - (changePerSecond * Time.unscaledDeltaTime), 0);
                } else if (currentLookOffsetDown > 0) {
                    float changePerSecond = lookUpCameraYOffset / cameraLookTime;
                    currentLookOffsetDown = Mathf.Max(currentLookOffsetDown - (changePerSecond * Time.unscaledDeltaTime), 0);
                }
            }

            if (CthulkInput.CameraLookRightToggle(cameraLookRightToggled)) {
                cameraLookRightToggled = !cameraLookRightToggled;
            }

            if (cameraLookRightToggled && !inShipView) {
                if (currentLookOffsetRight < lookRightCameraXOffset) {
                    float changePerSecond = lookRightCameraXOffset / cameraShiftTime;
                    currentLookOffsetRight = Mathf.Min(currentLookOffsetRight + (changePerSecond * Time.unscaledDeltaTime), lookRightCameraXOffset);
                }
            } else {
                if (currentLookOffsetRight > 0) {
                    float changePerSecond = lookRightCameraXOffset / cameraShiftTime;
                    currentLookOffsetRight = Mathf.Max(currentLookOffsetRight - (changePerSecond * Time.unscaledDeltaTime), 0);
                }
            }

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

        public void ForceCameraBehaviour(bool force, float x, float y, float size) {
            cameraBehaviourForced = force;
            currentCameraMovementOriginSize = cameraComponent.orthographicSize;
            currentCameraMovementOriginY = transform.position.y;
            currentCameraMovementOriginX = transform.position.x;

            cameraTargetX = x;
            cameraTargetY = y;
            cameraTargetSize = size;
            currentTargetTransitionDuration = cameraShiftTime;
            cameraMovementTargetEndTime = Time.realtimeSinceStartup + cameraShiftTime;
        }

        void UpdateShipViewOverride(bool exitShipViewIfExhausted = false) {
            if (shipViewOverrides.Count == 0) {
                CamLog("Removing ship view override -- no overrides remaining");
                shipViewForced = false;
                shipViewTarget = originalShipViewTarget;
                hasXPositionOverride = false;
                if (exitShipViewIfExhausted) {
                    shipViewHeld = false;
                }
            } else {
                CamLog("Updating ship view override -- " + shipViewOverrides.Count + " overrides");
                shipViewOverrides.Sort((x, y) => y.priority.CompareTo(x.priority));
                ForcedCamera highestPriorityOverride = shipViewOverrides[0];
                CamLog("Highest priority override is " + highestPriorityOverride.name);
                if (highestPriorityOverride.isCameraPositionOverride) {
                    shipViewTarget = highestPriorityOverride.target;
                } else {
                    shipViewTarget = originalShipViewTarget;
                }
                shipViewForced = true;
                hasXPositionOverride = highestPriorityOverride.isXPositionOverride;
                xPositionOverride = highestPriorityOverride.target.position.x;
            }
            if ((!inShipView && shipViewForced) || (inShipView && !shipViewForced) || (hasXPositionOverride)) {
                currentCameraMovementOriginX = transform.position.x;
                inShipViewTransition = true;
            }
            
        }

        public void AddShipViewOverride(string overrideName, int priority, bool lockControls = false) {
            ForcedCamera cameraOverride = new ForcedCamera(overrideName, originalShipViewTarget, priority, false, false, lockControls);
            shipViewOverrides.Add(cameraOverride);
            UpdateShipViewOverride();
        }

        public void AddShipViewOverride(string overrideName, int priority, Transform targetOverride, bool overrideXPosition = false, bool overrideCameraTarget = true, bool lockControls = false) {
            ForcedCamera cameraOverride = new ForcedCamera(overrideName, targetOverride, priority, overrideXPosition, overrideCameraTarget, lockControls);
            shipViewOverrides.Add(cameraOverride);
            UpdateShipViewOverride();
        }

        public void RemoveShipViewOverride(string overrideName, bool exitShipView = false) {
            shipViewOverrides.RemoveAll(x => x.name == overrideName);
            UpdateShipViewOverride(exitShipView);
        }

       
    }
}