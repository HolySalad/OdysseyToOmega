using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities.DNA;
using TMPro;
public class AvatarItem : MonoBehaviour
{
    //This goes on the avatar item prefab to initialize it
    private TotemDNADefaultAvatar asset;
    public void Setup(TotemDNADefaultAvatar asset)
    {
        //Operations to initialize the prefab
        //Asset gives info about the caracteristics
        Debug.Log("Hair style: " + asset.hair_styles);
        Debug.Log("PrimaryColor: " + asset.primary_color);
        Debug.Log("SecondaryColor: " + asset.secondary_color);

        //I have no idea what would you use this for
        this.asset = asset;
    }
}
