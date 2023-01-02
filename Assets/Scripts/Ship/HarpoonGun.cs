using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship {
        public class HarpoonGun : MonoBehaviour, IActivatables
    {
        [SerializeField] private GameObject harpoonPrefab;
        [SerializeField] private GameObject harpoonLocation;
        [SerializeField] private GameObject backBarrel;
        [SerializeField] private GameObject frontBarrel;
        [SerializeField] private float minAngle = -5;
        [SerializeField] private float maxAngle = 30;
        [SerializeField] private float RotationSpeed = 5f;

        public bool isLoaded {get; private set;} = false;

        public bool isInUse {get; private set;} = false;
        private bool returnPlayerToSmallCamera = false;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerState playerState {get;} = PlayerState.aiming;
        public string usageAnimation {get;} = "Repairing";



        public void Activate(Player player) {
            isInUse = true;
            if (!player.cameraControls.isShipView) {
                returnPlayerToSmallCamera = true;
                player.cameraControls.ToggleShipView();
            }
        }
        public void Deactivate(Player player) {
            isInUse = false;
            if (returnPlayerToSmallCamera) {
                player.cameraControls.ToggleShipView();
                returnPlayerToSmallCamera = false;
            }
        }

        public void Deactivate(Player player, bool internalDeactivation) {
            Deactivate(player);
            player.DetatchFromActivatable();
        }

        public bool ActivationCondition(Player player) {
            return isLoaded;
        }

        public void LoadHarpoon() {
            isLoaded = true;
            harpoonLocation.GetComponent<SpriteRenderer>().enabled = true;
        }

        public void FireHarpoon() {
            isLoaded = false;
            harpoonLocation.GetComponent<SpriteRenderer>().enabled = false;
            GameObject harpoon = Instantiate(harpoonPrefab, harpoonLocation.transform.position, harpoonLocation.transform.rotation);
            //Vector3 gunAxis = backBarrel.transform.position - frontBarrel.transform.position;
            Vector3 direction = harpoonLocation.transform.TransformDirection(Vector3.right);
            harpoon.GetComponent<Ship.HarpoonProjectile>().Fire(direction);
            SoundManager.Instance.Play("HarpoonWhoosh");
            Deactivate(GameModel.Instance.player, true);
        }

        public void Awake() {
            if (harpoonPrefab == null) {
                Debug.LogError("HarpoonGun: No harpoon prefab set on "+ this.gameObject.name);
            }
            if (harpoonLocation == null) {
                Debug.LogError("HarpoonGun: No harpoon location set on "+ this.gameObject.name);
            } else {
                harpoonLocation.GetComponent<SpriteRenderer>().enabled = isLoaded;
            }
        }

        public void Update() {
            if (isInUse) {
                Debug.DrawRay(harpoonLocation.transform.position, harpoonLocation.transform.TransformDirection(Vector3.right)*100, Color.red, Time.deltaTime );
                if (Input.GetKeyDown(KeyCode.Space)) {
                    FireHarpoon();
                    Deactivate(GameModel.Instance.player);
                }
                
                Quaternion target = Quaternion.Euler(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y,Mathf.Clamp(transform.rotation.eulerAngles.z + (Input.GetAxis("Horizontal") *-1 * RotationSpeed), minAngle, maxAngle));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, RotationSpeed);
            }
        }

    }
}