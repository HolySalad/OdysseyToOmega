using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {    
    public class ChydraBoss : MonoBehaviour, IHazardManager
    {
        public static ChydraBoss Instance { get; private set; }
        [Header("Test Mode")]
        [SerializeField] private bool testMode = false;
        [Header("Object References")]
        [SerializeField] private GameObject[] heads;
        [SerializeField] private GameObject parent;
        [SerializeField] private Collider2D shipInteriorCollider;
        [SerializeField] private Transform shipInteriorBlockTarget;
        [SerializeField] private Transform harpoonGunBlockTarget;

        [Header("Projectile Prefabs")]
        [SerializeField] public GameObject fireballPrefab;
        [SerializeField] public GameObject fireStreamPrefab;
        [SerializeField] public GameObject acidBallPrefab;

        [Header("Hazard Settings")]
        [SerializeField] private float firstTargetSailTime = 10f;
        [SerializeField] private float targetSailsInterval = 30f;
        [SerializeField] private float blockHarpoonInterval = 30f;

        private int currentStage = 0;
        private int currentHeadsAlive = 0;

        public string HazardSoundtrack {get;} = "";
        public HazardTypes HazardType {get;} = HazardTypes.HydraBoss;

        public bool HasEnded { get; private set; }
        public bool WasCompleted { get; private set; } = false;

        private float hazardBeganTime = -1f;      
        public float HazardTime() {
            return Time.time - hazardBeganTime;
        }

        private bool hasBlockedShipInterior = false;
        public bool ShouldBlockShipInterior() {
            return (!hasBlockedShipInterior) && !shipInteriorCollider.OverlapPoint(GameModel.Instance.player.transform.position);
        }
        public void BlockedShipInterior() {
            hasBlockedShipInterior = true;
        }

        private float lastSailTargetTime = -1f;
        private float lastHarpoonBlockTime = -1f;

        public bool ShouldTargetSails() {
            return HazardTime() - lastSailTargetTime > targetSailsInterval;
        }
        public void TargettedSails() {
            lastSailTargetTime = HazardTime();
        }

        public bool ShouldBlockHarpoon() {
            return HazardTime() - lastHarpoonBlockTime > blockHarpoonInterval;
        }
        public void BlockedHarpoon() {
            lastHarpoonBlockTime = HazardTime();
        }




        void FixedUpdate() {
            if (hazardBeganTime < 0 || HasEnded) {
                return;
            }
            if (currentStage == 3 && currentHeadsAlive == 0) {
                HasEnded = true;
                WasCompleted = true;
            }
            if (currentStage == 0) {

            }
        }

        public void StartHazard(HazardDifficulty difficulty) {
            currentStage = 0;
            currentHeadsAlive = 1;
            lastSailTargetTime = firstTargetSailTime - targetSailsInterval;
            heads[0].GetComponent<Enemies.ChydraNew.ChydraController>().ActivateController(this);
        }

        void Awake() {
            Instance = this;
        }

        void Start() {
            if (testMode) {
                StartHazard(HazardDifficulty.Easy);
            }
        }
    }
}