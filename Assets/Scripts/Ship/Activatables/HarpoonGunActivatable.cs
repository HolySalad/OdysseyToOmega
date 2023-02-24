using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship.Activatables {
    public class HarpoonGunActivatable : MonoBehaviour, IActivatables
    {   
        [SerializeField] private UI.HelpPrompt helpPrompt;
        public UI.HelpPrompt activatableHelpPrompt {get {return helpPrompt;}}

        [SerializeField] private UI.HelpPrompt inUseHelpPrompt;
        public UI.HelpPrompt activatableInUseHelpPrompt {get {return inUseHelpPrompt;}}
        [SerializeField] private GameObject harpoonPrefab;
        [SerializeField] private GameObject harpoonLocation;
        [SerializeField] private GameObject harpoonBarrel;
        [SerializeField] private GameObject AimerParent;
        [SerializeField] private GameObject Aimer1;
        [SerializeField] private GameObject Aimer2;
        [SerializeField] private Transform overrideShipViewTarget;
        [SerializeField] private float minAngle = -5;
        [SerializeField] private float maxAngle = 30;
        [SerializeField] private float RotationSpeed = 5f;
        [SerializeField] private int maxAimerDistance = 100;



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
            AimerParent.SetActive(true);
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
            AimerParent.SetActive(false);
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
            Debug.Log("Deactivating harpoon gun by player");
            StartCoroutine(RestoreCamera(0.2f));
        }

        public void Deactivate(Player player, bool internalDeactivation) {
            Debug.Log("Deactivating harpoon gun by harpoon");
            AimerParent.SetActive(false);
            isInUse = false;
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
            player.DetatchFromActivatable();
            StartCoroutine(RestoreCamera(2f));
        }

        public bool ActivationCondition(Player player) {
            return isLoaded;
        }

        public void LoadHarpoon() {
            isLoaded = true;
            harpoonLocation.GetComponent<SpriteRenderer>().enabled = true;
        }

        void Start() {
            Quaternion rot = Aimer1.transform.rotation;
            Vector3 posDiff = Aimer2.transform.position - Aimer1.transform.position;
            Vector3 lastPos = Aimer2.transform.position;
            for (int i = 0; i < maxAimerDistance; i++) {
                GameObject aimer = Instantiate(Aimer1, AimerParent.transform);
                aimer.transform.position = lastPos + posDiff;
                lastPos = aimer.transform.position;
                aimer.transform.rotation = rot;
            }
            AimerParent.SetActive(false);
        }

        public void FireHarpoon() {
            isLoaded = false;
            harpoonLocation.GetComponent<SpriteRenderer>().enabled = false;
            GameObject harpoon = Instantiate(harpoonPrefab, harpoonLocation.transform.position, harpoonLocation.transform.rotation);
            //Vector3 gunAxis = backBarrel.transform.position - frontBarrel.transform.position;
            Vector3 direction = harpoonLocation.transform.TransformDirection(Vector3.right);
            HarpoonProjectile harpoonProjectile = harpoon.GetComponent<Ship.Activatables.HarpoonProjectile>();
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
                }
                float rotation = (CthulkInput.HorizontalInput() *-1 * RotationSpeed * Time.deltaTime);
                if (rotation == 0) {
                    return;
                } else if (rotation > 0) {
                } else {
                }
                float originalRotation = transform.rotation.eulerAngles.z;
                if (originalRotation > 180) {
                    originalRotation = originalRotation - 360;
                }
                float clampedValue = Mathf.Clamp(originalRotation + rotation, minAngle, maxAngle);
                if (clampedValue < 0) {
                    clampedValue = 360 + clampedValue;
                }
                Debug.Log("Original rotation: " + originalRotation + " Rotation: " + rotation + " Clamped Value: " + clampedValue);
                Quaternion target = Quaternion.Euler(transform.rotation.eulerAngles.x,transform.rotation.eulerAngles.y, clampedValue);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, RotationSpeed);
                harpoonBarrel.transform.rotation = Quaternion.RotateTowards(harpoonBarrel.transform.rotation, target, RotationSpeed);
            }
        }

    }
}