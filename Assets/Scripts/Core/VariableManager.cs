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

    private TotemDNADefaultAvatar avatar;

    public TotemDNADefaultAvatar Avatar {
        get {
            return avatar;
        }
        set {
            avatar = value;
        }
    }
    private bool defaultHarpoon = true;
    public bool DefaultHarpoon {
        get {
            return defaultHarpoon;
        }
        set {
            if (value) {
                settingsSavedData.saveData.harpoonSelection = "Javier";
            }
            defaultHarpoon = value;
        }
    }

    private bool defaultAvatar = true;
    public bool DefaultAvatar {
        get {
            return defaultAvatar;
        }
        set {
            if (value) {
                settingsSavedData.saveData.avatarSelection = "Javier";
            }
            defaultAvatar = value;
        }
    }


    private TotemDNADefaultItem harpoon;
    public TotemDNADefaultItem Harpoon {
        get {
            return harpoon;
        }
        set {
            harpoon = value;
        }
    }

    public void SetTotemUser(string publicKey) {
        totemLoginLast = publicKey;
        if (settingsSavedData.saveData.totemLoginPublicKey == publicKey) {

        } else {
            settingsSavedData.saveData.avatarSelection = "fuckoff";
            settingsSavedData.saveData.harpoonSelection = "fuckoff";
            settingsSavedData.saveData.totemLoginPublicKey = publicKey;
            settingsSavedData.Save();
        }
    }

    private string totemLoginLast;
    public bool IsLoggedInWithTotem() {
        return totemLoginLast != null;
    }

    public static VariableManager Instance { get; private set; }
    public SpaceBoat.SaveDataManager settingsSavedData;
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

    public void SaveAvatars() {
        settingsSavedData.saveData.avatarSelection = avatar.ToString();
        if (!defaultHarpoon) settingsSavedData.saveData.harpoonSelection = harpoon.ToString();
        settingsSavedData.Save();
        Debug.Log("Saved Avatar to: " + settingsSavedData.saveData.avatarSelection + " " + settingsSavedData.saveData.harpoonSelection);
    }
}
