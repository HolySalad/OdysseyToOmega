using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship {
        public class HarpoonGunActivatable : MonoBehaviour, IActivatables
    {
        [SerializeField] private GameObject harpoonPrefab;
        [SerializeField] private GameObject harpoonLocation;
        [SerializeField] private GameObject backBarrel;
        [SerializeField] private GameObject frontBarrel;
        [SerializeField] private Transform overrideShipViewTarget;
        [SerializeField] private float minAngle = -5;
        [SerializeField] private float maxAngle = 30;
        [SerializeField] private float RotationSpeed = 5f;



        public bool isLoaded {get; private set;} = true;
        public ActivatablesNames kind {get;} = ActivatablesNames.HarpoonGun;

        public bool isInUse {get; private set;} = false;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerStateName playerState {get;} = PlayerStateName.turret;
        public string usageAnimation {get;} = "Repairing";
        public string usageSound {get;}

        private List<UsageCallback> usageCallbacks = new List<UsageCallback>();
        private List<UsageCallback> deactivationCallbacks = new List<UsageCallback>();
        public void AddActivationCallback(UsageCallback callback) {
            usageCallbacks.Add(callback);
        }
        public void AddDeactivationCallback(UsageCallback callback) {
            deactivationCallbacks.Add(callback);
        }

        public void Activate(Player player) {
            isInUse = true;            
            foreach (UsageCallback callback in usageCallbacks) {
                callback();
            }
            GameModel.Instance.cameraController.AddShipViewOverride("HarpoonAim", 10, overrideShipViewTarget, true);
        }

        IEnumerator RestoreCamera(float delay) {
            yield return new WaitForSeconds(delay);
            GameModel.Instance.cameraController.RemoveShipViewOverride("HarpoonAim");
        }

        public void Deactivate(Player player) {
            isInUse = false;
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
            StartCoroutine(RestoreCamera(0.2f));
        }

        public void Deactivate(Player player, bool internalDeactivation) {
            isInUse = false;
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
            player.DetatchFromActivatable();
            StartCoroutine(RestoreCamera(2.5f));
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
            HarpoonProjectile harpoonProjectile = harpoon.GetComponent<Ship.HarpoonProjectile>();
            harpoonProjectile.Fire(direction);
            harpoonProjectile.harpoonGun = this;
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
                if (CthulkInput.EquipmentUsageKeyDown()) {
                    FireHarpoon();
                    Deactivate(GameModel.Instance.player);
                }
                
                Quaternion target = Quaternion.Euler(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y,Mathf.Clamp(transform.rotation.eulerAngles.z + (CthulkInput.HorizontalInput() *-1 * RotationSpeed), minAngle, maxAngle));
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, RotationSpeed);
            }
        }

    }
}