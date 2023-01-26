using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {    
    public class CthulkInput {

        
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

        public static float HorizontalInput() {
            return Input.GetAxisRaw("Horizontal"); 
        }

        public static bool CameraToggleDown() {
            return Input.GetKeyDown(KeyCode.Tab);
        }

        public static bool CameraLookRightHeld() {
            return Input.GetKey(KeyCode.R);
        }

        public static bool cameraLookDownHeld() {
            return Input.GetKey(KeyCode.C);
        }
    }
}