using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers{
    public class HydraBoss : MonoBehaviour, IHazardManager
    {
        [SerializeField] private GameObject hydraPrefab;
        [SerializeField] private Transform hydraSpawnPoint;
        bool hydraStarted = false;
        bool hydraDead = false;
        
        public string HazardSoundtrack {get;} = "";
        public bool HasEnded { get; private set; } = false;
        public bool WasCompleted {get;} = false;
        public HazardTypes HazardType {get;} = HazardTypes.HydraBoss;
        

        private float hazardBeganTime = -1f;

        private GameObject hydra;

        private void Start()
        {
            hydraPrefab = GameModel.Instance.bossParent;
        }
        private void Update()
        {
            if (hydraStarted && hydra.activeInHierarchy == false && hydraDead == false)
            {
                HasEnded = true;
                hydraDead = true;
            GameModel.Instance.cameraController.RemoveShipViewOverride("Hydra");
            }
        }

        public void StartHazard(HazardDifficulty difficulty) {
            hydraPrefab = GameModel.Instance.bossParent;
            hazardBeganTime = Time.time;

            GameModel.Instance.cameraController.AddShipViewOverride("Hydra", 999);
            hydraStarted = true;
            hydra = hydraPrefab;
            hydra.transform.position = hydraSpawnPoint.position;
            hydra.SetActive(true);

        } 




        
    }
}