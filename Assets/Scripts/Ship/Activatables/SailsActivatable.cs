using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship.Activatables {
    public class SailsActivatable : MonoBehaviour, IActivatables
    {
        public enum SailGrouping {
            WheelSails,
            MainSails,
            SmallSails,
        }

        [SerializeField] private Sprite repairedSprite;
        [SerializeField] private Sprite brokenSprite;
        [SerializeField] public Transform hazardTarget;
        [SerializeField] private float repairTime = 2.2f;
        [SerializeField] private float targettingCooldown = 30f;
        [SerializeField] private float targetFlagTimeout = 10f;
        [SerializeField] private bool breakOnStart = false;
        [SerializeField] public SailGrouping sailGrouping = SailGrouping.SmallSails;

        public ActivatablesNames kind {get;} = ActivatablesNames.Sails;
        public bool isInUse {get; private set;} = false;
        private SpriteRenderer spriteRenderer;
        public bool isBroken {get; private set;} = false;
        public bool isTargetted {get; private set;} = false;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerStateName playerState {get;} = PlayerStateName.working;
        public string usageAnimation {get;} = "Repairing";
        public string usageSound {get;} = "Repair";


        private float timeBeganRepairing = 0;
        public float lastTargettedTime = -99f;

        private Player player;

        private List<UsageCallback> usageCallbacks = new List<UsageCallback>();
        private List<UsageCallback> deactivationCallbacks = new List<UsageCallback>();
        private List<UsageCallback> onNextSailRepairCallbacks = new List<UsageCallback>();
        public void AddActivationCallback(UsageCallback callback) {
            usageCallbacks.Add(callback);
        }
        public void AddDeactivationCallback(UsageCallback callback) {
            deactivationCallbacks.Add(callback);
        }
        public void AddOnSailRepairCallback(UsageCallback callback) {
            onNextSailRepairCallbacks.Add(callback);
        }

        void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (repairedSprite == null) {
                Debug.LogError("Sails: No repaired sprite set on "+ this.gameObject.name);
            }
            if (brokenSprite == null) {
                Debug.LogError("Sails: No broken sprite set on "+ this.gameObject.name);
            }
        }

        void Start() {
            if (breakOnStart) {
                Break();
            }
        }

        public void Repair() {
            isBroken = false;
            spriteRenderer.sprite = repairedSprite;
            foreach (UsageCallback callback in onNextSailRepairCallbacks) {
                callback();
            }
        }

        public void Break() {
            isBroken = true;
            isTargetted = false;
            spriteRenderer.sprite = brokenSprite;
        }

        public void Activate(Player player) {
            this.player = player;
            isInUse = true;
            timeBeganRepairing = Time.time;
            foreach (UsageCallback callback in usageCallbacks) {
                callback();
            }
        }

        public bool IsOnCooldown() {
            return Time.time -lastTargettedTime < targettingCooldown;
        }

        public void TargetSail() {
            isTargetted = true;
            lastTargettedTime = Time.time;
        }

        public void Deactivate(Player player) {
            isInUse = false;
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
        }

        public bool ActivationCondition(Player player) {
            return isBroken;
        }

        void OnTriggerExit2D(Collider2D other) {
            if (isInUse && other.gameObject.tag == "Player") {
                Deactivate(player);
                player.DetatchFromActivatable();
            }
        }

        void Update() {
            if (isInUse && isBroken) {
                if (Time.time - timeBeganRepairing >= repairTime) {
                    Repair();
                    Deactivate(player);
                    player.DetatchFromActivatable();
                }
            }
            if (isTargetted && Time.time - lastTargettedTime > targetFlagTimeout) {
                isTargetted = false;
            }
        }
    }
}