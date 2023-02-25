using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities.DNA;
using TMPro;

public class ItemItem : MonoBehaviour
{
    //This goes on the avatar item prefab to initialize it
    private TotemDNADefaultItem asset;
    public void Setup(TotemDNADefaultItem asset)
    {
        //Operations to initialize the prefab
        //Asset gives info about the caracteristics
        Debug.Log("Element: " + asset.classical_element);
        Debug.Log("Material: " + asset.weapon_material);
        Debug.Log("PrimaryColor: " + asset.primary_color);
        Debug.Log("SecondaryColor: " + asset.secondary_color);

        //I have no idea what would you use this for
        this.asset = asset;
    }
}
