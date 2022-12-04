using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers {
    public interface IHazardManager
    {
        public void StartHazard();
        public float hazardDuration {get;}
    }
}