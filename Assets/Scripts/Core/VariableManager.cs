using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities.DNA;

public class VariableManager : MonoBehaviour
{
    private float generalVolume = 1;
    public float GeneralVolume {get {
        return generalVolume;
    } set {
            if (SoundManager.Instance != null) {
                SoundManager.Instance.SetMasterVolume(value);
            }
            generalVolume = value;
        }
    }
    private float musicVolume = 1;
    public float MusicVolume {get {
        return musicVolume;
    } set {
            if (SoundManager.Instance != null) {
                SoundManager.Instance.SetMusicVolume(value);
            }
            musicVolume = value;
        }
    }
    private float effectsVolume = 1;
    public float EffectsVolume {
        get {
            return effectsVolume;
    } set {
            if (SoundManager.Instance != null) {
                SoundManager.Instance.SetSFXVolume(value);
            }
            effectsVolume = value;
        }
    }

    public bool resetGame = false;

    public TotemDNADefaultAvatar avatar = null;
    public TotemDNADefaultItem harpoon = null;

    public static VariableManager Instance { get; private set; }
    private SpaceBoat.SaveDataManager settingsSavedData;
    void Awake(){
    if (Instance != null && Instance != this) 
    { 
        Destroy(this.gameObject); 
    } 
    else 
    { 
        Instance = this; 
    }
        settingsSavedData = new SpaceBoat.SaveDataManager("/SpaceBoatSettings.json");
        settingsSavedData.Load();
        Debug.Log("Loaded Settings: " + settingsSavedData.saveData.generalVolume + " " + settingsSavedData.saveData.musicVolume + " " + settingsSavedData.saveData.effectsVolume);
        GeneralVolume = settingsSavedData.saveData.generalVolume;
        MusicVolume = settingsSavedData.saveData.musicVolume;
        EffectsVolume = settingsSavedData.saveData.effectsVolume;
        DontDestroyOnLoad(this.gameObject);
    }

    public void SaveSettings() {
        settingsSavedData.saveData.generalVolume = generalVolume;
        settingsSavedData.saveData.musicVolume = musicVolume;
        settingsSavedData.saveData.effectsVolume = effectsVolume;
        settingsSavedData.Save();
        Debug.Log("Saved Settings to: " + settingsSavedData.saveData.generalVolume + " " + settingsSavedData.saveData.musicVolume + " " + settingsSavedData.saveData.effectsVolume);
    }
}
