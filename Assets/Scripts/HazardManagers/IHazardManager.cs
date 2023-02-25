using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {
    public enum HazardTypes {MeteorShower, CosmicStorm, BugSwarm, HydraBoss, None}
    public enum HazardDifficulty {Easy, Medium, Hard}

    [System.Serializable] public class HazardDefinition {
        public HazardTypes hazardType;
        public GameObject hazardManagerPrefab;
    }

    [System.Serializable] public class HazardPlanner {
        public List<HazardOptions> hazardPlan;
    }

    [System.Serializable] public class HazardOptions {
        public HazardDifficulty difficulty;
        public List<HazardTypes> hazardOptions;
    }


    public interface IHazardManager
    {
        public GameObject gameObject {get;}
        public void StartHazard(HazardDifficulty difficulty);

        public string HazardSoundtrack {get;}
        
        public float HazardDuration {get;}
        public bool HasEnded {get;} 
        public bool WasCompleted {get;}
        public HazardTypes HazardType {get;}
    }
}