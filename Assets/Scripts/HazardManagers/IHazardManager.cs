using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {
    public interface IHazardManager
    {
        public GameObject gameObject {get;}
        public void StartHazard();

        public string hazardSoundtrack {get;}
        
        public float hazardDuration {get;}
        public bool hasEnded {get;} 
        public bool wasCompleted {get;}

        public int GetPriority();
        public int GetEarliestAppearence();
        public int GetLatestAppearence();
    }
}