using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities.DNA;

public class VariableManager : MonoBehaviour
{
    public float generalVolume = 1;
    public float musicVolume = 1;
    public float effectsVolume = 1;

    public bool resetGame = false;

    public TotemDNADefaultAvatar avatar = null;
    public TotemDNADefaultItem harpoon = null;

    public static VariableManager Instance { get; private set; }
    void Awake(){
    if (Instance != null && Instance != this) 
    { 
        Destroy(this.gameObject); 
    } 
    else 
    { 
        Instance = this; 
    }

        DontDestroyOnLoad(this.gameObject);
    }
}
