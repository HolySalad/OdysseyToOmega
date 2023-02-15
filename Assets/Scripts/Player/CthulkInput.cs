using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {    
    public class CthulkInput {

        public static bool AttackKeyDown() {
            return Input.GetButtonDown("Attack");
        }

        public static bool AttackKeyHeld() {
            return Input.GetButton("Attack");
        }
        
        public static bool EquipmentUsageKeyDown() {
            return Input.GetButtonDown("Ability");
        }
        public static bool EquipmentUsageKeyHeld() {
            return Input.GetButton("Ability");
        }

        public static bool JumpKeyDown() {
            return Input.GetButtonDown("Jump");
        }

        public static bool JumpKeyHeld() {
            return Input.GetButton("Jump");
        }

        public static bool CrouchHeld() {
            return Input.GetButton("Crouch");
        }

        public static bool ActivateKeyDown() {
            return Input.GetButtonDown("Activate");
        }

        public static bool ActivateKeyHeld() {
            return Input.GetButton("Activate");
        }


        public static float HorizontalInput() {
            return Input.GetAxisRaw("Horizontal"); 
        }

        public static bool CameraToggleDown() {
            return Input.GetButtonDown("ToggleShipCamera") || Input.GetAxisRaw("ToggleShipCameraAxis") != 0;
        }

        public static bool CameraLookRightToggle(bool alreadyHeld = false) {
            if (!alreadyHeld) {
                return Input.GetButtonDown("LookRight") || Input.GetAxisRaw("LookRightAxis") > 0;
            } else {
                return Input.GetButtonDown("LookRight") || Input.GetAxisRaw("LookRightAxis") < 0;
            }
        }

        public static float cameraVerticalLook() {
            return Input.GetAxisRaw("LookVertical");
        }
    }
}