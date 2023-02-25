using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers{
    public class HydraBoss : MonoBehaviour, IHazardManager
    {
        [SerializeField] private GameObject hydraPrefab;
        [SerializeField] private Transform hydraSpawnPoint;
        
        
        public string HazardSoundtrack {get;} = "";
        public bool HasEnded {get;} = false;
        public bool WasCompleted {get;} = false;
        public HazardTypes HazardType {get;} = HazardTypes.HydraBoss;
        

        private float hazardBeganTime = -1f;

        private GameObject hydra;


        public void StartHazard(HazardDifficulty difficulty) {
            hazardBeganTime = Time.time;

            hydra = Instantiate(hydraPrefab, hydraSpawnPoint.position, Quaternion.identity);
        } 




        
    }
}