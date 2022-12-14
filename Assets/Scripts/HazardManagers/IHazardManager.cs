using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {
    public interface IHazardManager
    {
        public GameObject gameObject {get;}
        public void StartHazard();
        
        public float hazardDuration {get;}
        public bool hasEnded {get;}
    }
}