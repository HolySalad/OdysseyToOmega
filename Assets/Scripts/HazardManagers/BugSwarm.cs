using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Ship.Activatables;
using SpaceBoat.HazardManagers.BugSwarmSubclasses;

namespace SpaceBoat.HazardManagers.BugSwarmSubclasses {
    [System.Serializable] public class EscalationLevel {
        public float timeIntoHazard = 0f;
        public float maxNumBugs = 0f;
        public float bugSpawnIntervalMultiplier = 1f;
        public float bomberBugSpawnIntervalMultiplier = 1f;
        public int numBomberBugs = 0;
    }

    [System.Serializable] public class EscalationSettings {
        public List<EscalationLevel> escalationLevelsEasy;
        public List<EscalationLevel> escalationLevelsMedium;
        public List<EscalationLevel> escalationLevelsHard;

        public List<EscalationLevel> GetEscalationLevels(HazardDifficulty difficulty) {
            switch (difficulty) {
                case HazardDifficulty.Easy:
                    return escalationLevelsEasy;
                case HazardDifficulty.Medium:
                    if (escalationLevelsMedium.Count == 0) return GetEscalationLevels(HazardDifficulty.Easy);
                    return escalationLevelsMedium;
                case HazardDifficulty.Hard:
                    if (escalationLevelsHard.Count == 0) return GetEscalationLevels(HazardDifficulty.Medium);
                    return escalationLevelsHard;
                default:
                    return escalationLevelsEasy;
            }
        }
    }
}


namespace SpaceBoat.HazardManagers {
    public class BugSwarm : MonoBehaviour, IHazardManager
    {
        [SerializeField] private bool isTestMode;
        [SerializeField] private EscalationSettings escalationSettings;
        [SerializeField] private float baseHazardDuration = 135f;

        [SerializeField] private GameObject roboBugPrefab;
        [SerializeField] private Transform bugSpawnPoint;
        [SerializeField] private Transform bugExitPoint;

        [SerializeField] private Transform[] bugTargetLocations;

        [SerializeField] private float baseBugSpawnInterval = 10f;
        [SerializeField] private float firstBugSpawnTime = 5f;
        [SerializeField] private float baseBomberBugSpawnInterval = 30f;
        [SerializeField] private float bomberBugSpawnBuffer = 1.5f;

        public HazardTypes HazardType {get;} = HazardTypes.BugSwarm;

        private int nextEscalationIndex = 0;
        private EscalationLevel currentEscalationLevel;
        private List<EscalationLevel> escalationLevels;
        

        public string HazardSoundtrack {get;} = "SwarmGalaxy";
        public float HazardDuration {get; private set;} = 0f;

        public bool HasEnded {get; private set;} = false;
        public bool WasCompleted {get; private set;} = false;
        private float hazardBeganTime = -1f;       

        private List<RoboBug> roboBugs = new List<RoboBug>();
        private Dictionary<Transform, bool> bugTargetLocationsOccupied = new Dictionary<Transform, bool>();
        private Dictionary<RoboBug, Transform> bugTargetLocationsAssigned = new Dictionary<RoboBug, Transform>();
        private List<GameObject> bomberBugTargets = new List<GameObject>();

        private float lastBugSpawnTime = -1f;
        private float lastBomberBugSpawnTime = -1f;
        private bool bugSpawnsEnabled = false;

        public void RemoveBugFromSwarm(RoboBug bug) {
            roboBugs.Remove(bug);
            if (!bugTargetLocationsAssigned.ContainsKey(bug)) return;
            Transform targetLocation = bugTargetLocationsAssigned[bug];
            bugTargetLocationsOccupied[targetLocation] = false;
            bugTargetLocationsAssigned.Remove(bug);
        }

        float HazardTime() {
            return Time.time - hazardBeganTime;
        }

        void Awake() {
            HazardDuration = baseHazardDuration;
            foreach (Transform targetLocation in bugTargetLocations) {
                bugTargetLocationsOccupied.Add(targetLocation, false);
            }
        }
        



        void SpawnBug(bool isBomber) {
            GameObject bug = Instantiate(roboBugPrefab, bugSpawnPoint.position, Quaternion.identity);
            RoboBug roboBug = bug.GetComponent<RoboBug>();
            List<Transform> availableTargetLocations = new List<Transform>();
            foreach (Transform targetLocation in bugTargetLocations) {
                if (!bugTargetLocationsOccupied[targetLocation]) {
                    availableTargetLocations.Add(targetLocation);
                }
            }
            if (availableTargetLocations.Count == 0) {
                Debug.LogError("No available target locations for bug swarm hazard " + this.gameObject.name);
                return;
            }
            Transform targetLocationToAssign = availableTargetLocations[Random.Range(0, availableTargetLocations.Count)];
            bugTargetLocationsOccupied[targetLocationToAssign] = true;
            bugTargetLocationsAssigned.Add(roboBug, targetLocationToAssign);
            roboBug.SetupRobobug(this, targetLocationToAssign.position, bugExitPoint);
            roboBugs.Add(roboBug);
            lastBugSpawnTime = HazardTime();
            if (isBomber) {
                if (bomberBugTargets.Count == 0) {
                    Debug.LogError("No bomber bug targets set for hazard " + this.gameObject.name);
                }
                int randomTargetIndex = Random.Range(0, bomberBugTargets.Count);
                lastBomberBugSpawnTime = HazardTime();
                roboBug.SetupBomber(bomberBugTargets[randomTargetIndex]);
                bomberBugTargets.RemoveAt(randomTargetIndex);
            }
        }

        IEnumerator CheckShouldSpawnBugs() {
            while (!HasEnded) {
                if (roboBugs.Count < Mathf.Min(currentEscalationLevel.maxNumBugs, bugTargetLocations.Length)) {
                    if (HazardTime() > lastBomberBugSpawnTime + (baseBomberBugSpawnInterval * currentEscalationLevel.bomberBugSpawnIntervalMultiplier)) {
                        bomberBugTargets = GameModel.Instance.SelectSailsForTargetting(currentEscalationLevel.numBomberBugs);
                        for (int i = 0; i < bomberBugTargets.Count; i++) {
                            SpawnBug(true);
                            yield return new WaitForSeconds(bomberBugSpawnBuffer);
                        }
                    } else if (HazardTime() > lastBugSpawnTime + (baseBugSpawnInterval * currentEscalationLevel.bugSpawnIntervalMultiplier)) {
                        SpawnBug(false);
                    }
                }
                yield return null;
            }
        }
    

       public void StartHazard(HazardDifficulty difficulty) {
            hazardBeganTime = Time.time;
            escalationLevels = escalationSettings.GetEscalationLevels(difficulty);
       }

       void Start() {
            if (escalationSettings.escalationLevelsEasy.Count == 0) {
                Debug.LogError("No escalation levels set for hazard " + this.gameObject.name);
                return;
            }
            if (isTestMode) {
                StartHazard(HazardDifficulty.Easy);
            }
       }
        private bool finalEscalationLevelReached = false;
       void FixedUpdate() {
            if (hazardBeganTime < 0) {
                return;
            } else if (HasEnded) {
                return;
            }
            else if (HazardTime() > HazardDuration) {
                SoundManager.Instance.Stop("BugBuzz", true);
                HasEnded = true;
                foreach (RoboBug bug in roboBugs) {
                    bug.Explode();
                }
            }
            float timeSinceStart = HazardTime();
            
            if (nextEscalationIndex < escalationLevels.Count && timeSinceStart > escalationLevels[nextEscalationIndex].timeIntoHazard) {
                Debug.Log("Escalating hazard " + this.gameObject.name + " to level " + nextEscalationIndex);
                currentEscalationLevel = escalationLevels[nextEscalationIndex];
                nextEscalationIndex++;
            } else if (!finalEscalationLevelReached &&nextEscalationIndex == escalationLevels.Count) {
               finalEscalationLevelReached = true;
               Debug.Log("Escalating hazard " + this.gameObject.name + " reached its final level");
            }
            if (!bugSpawnsEnabled && timeSinceStart >firstBugSpawnTime) {
                bugSpawnsEnabled = true;
                StartCoroutine(CheckShouldSpawnBugs());
                SoundManager.Instance.Play("BugBuzz", 0.2f, true);
            }

       }
    }
}