using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Rewards;
using SpaceBoat.PlayerSubclasses.Equipment;
using SpaceBoat.HazardManagers;
using Newtonsoft.Json;
using System.IO;

namespace SpaceBoat {
    // subclasses for saving and loading
    [System.Serializable] public class SaveData {
        public int money = 0;
        public Dictionary<RewardType, bool> rewardsUnlocked = new Dictionary<Rewards.RewardType, bool>() {
            {RewardType.DashEquipmentBlueprint, false},
            {RewardType.HarpoonGunBuildableBlueprint, false},
            {RewardType.HarpoonLauncherEquipmentBlueprint, false},
            {RewardType.ShieldEquipmentBlueprint, false},
            {RewardType.HealthPackEquipmentBlueprint, false},
            {RewardType.JumpPadBuildableBlueprint, false},
            {RewardType.ShipShieldBuildableBlueprint, false}
        };

        public Dictionary<EquipmentType, bool> equipmentBuilt = new Dictionary<EquipmentType, bool>() {
            {EquipmentType.Dash, false},
            {EquipmentType.HarpoonLauncher, false},
            {EquipmentType.Shield, false},
            {EquipmentType.HealthPack, false}
        };

        public List<Ship.Buildables.buildableSaveData> buildables = new List<Ship.Buildables.buildableSaveData>();

        public Dictionary<HazardTypes, bool> hazardsCompleted = new Dictionary<HazardTypes, bool>();

        public bool movementTutorialPlayed;
        public bool cometTutorialPlayed;
        public bool craftingTutorialPlayed;
        public bool equipmentTutorialPlayed;
        public bool crouchTutorialPlayed;
        public bool tutorialHazardPlayed;

        public float generalVolume = 1;
        public float musicVolume = 1;
        public float effectsVolume = 1;
    }

    public class SaveDataManager {

        public SaveData saveData { get; private set; }
        private string saveDataPath = "/SpaceBoatSave.json";

        string SaveDataPath() {
            return Application.persistentDataPath + saveDataPath;
        }
        
        public SaveDataManager(string saveDataPath) {
            this.saveDataPath = saveDataPath;
            saveData = new SaveData();
        }

        public void Reset() {
            saveData = new SaveData();
            Debug.Log("Reset save data - " + SaveDataPath());
            Save();
        }

        public void ResetBetweenRuns() {
            saveData.equipmentBuilt = new Dictionary<EquipmentType, bool> {
                {EquipmentType.Dash, false},
                {EquipmentType.HarpoonLauncher, false},
                {EquipmentType.Shield, false},
                {EquipmentType.HealthPack, false}
            };
            saveData.hazardsCompleted.Clear();
            saveData.money = 0;
            Save();
        }

        public void Save() {
            using (StreamWriter writer = new StreamWriter(SaveDataPath())) {
                string data = JsonConvert.SerializeObject(saveData);
                writer.Write(data);
            }
        }

        public void Load() {
            if (File.Exists(SaveDataPath())) {
                using (StreamReader reader = new StreamReader(SaveDataPath())) {
                    string data = reader.ReadToEnd();
                    saveData = JsonConvert.DeserializeObject<SaveData>(data);
                }
            }
        }
    }
}
