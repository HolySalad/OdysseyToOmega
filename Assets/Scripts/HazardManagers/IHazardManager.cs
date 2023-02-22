using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {
    public enum HazardTypes {MeteorShower, CosmicStorm, BugSwarm, HydraBoss}
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

        public string hazardSoundtrack {get;}
        
        public float hazardDuration {get;}
        public bool hasEnded {get;} 
        public bool wasCompleted {get;}
        public HazardTypes hazardType {get;}
    }
}