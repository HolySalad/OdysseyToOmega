using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TotemEntities.DNA;

public class AvatarList : MonoBehaviour
{
    //Content objct
    [SerializeField] private Transform avatarsParent;
    //Prefab for the avatars instantiated
    //[SerializeField] private GameObject avatarPrefab;
    List<GameObject> userAvatars = new List<GameObject>();

    [Tooltip("Chtulk preview sprites")]
    [SerializeField] private List<Sprite> hairstyle = new List<Sprite>();
    [Tooltip("Sader for changin color")]
    [SerializeField] private Shader shader;
    [SerializeField] private GameObject avatarPH;
    private int currentAvatarID;

    public void BuildList(List<TotemDNADefaultAvatar> assets)
    {
        foreach (var asset in assets)
        {
            //Create one game object for each avatar
            GameObject currentAvatar = new GameObject("Avatar");
            currentAvatar.AddComponent<RectTransform>();
            currentAvatar.AddComponent<Image>();
            RectTransform rectTransform = currentAvatar.AddComponent<RectTransform>();
            rectTransform = avatarPH.GetComponent<RectTransform>();
            currentAvatar.GetComponent<Image>().sprite = SetHair(asset);
            Material mat = currentAvatar.GetComponent<Image>().material = new Material(shader);
            mat.SetColor("_BasePrimaryColour", asset.primary_color);
            mat.SetColor("_BaseEyeColour", asset.secondary_color);
            userAvatars.Add(currentAvatar);

        }
        ChangeAvatar(0);
    }

    private Sprite SetHair(TotemDNADefaultAvatar asset){
        if(hairstyle[0] != null){
            switch(asset.hair_styles){
                case "asymmetrical":
                    return hairstyle[0];
                case "braids":
                    return hairstyle[1];
                case "buzzCut":
                    return hairstyle[2];
                case "dreadlocks":
                    return hairstyle[3];
                case "long":
                    return hairstyle[4];
                case "ponytail":
                    return hairstyle[5];
                case "short":
                    return hairstyle[6];
                default:
                    return hairstyle[6];
            }
        }
        else{
            Debug.LogError("You have to set the previews for the avatars");
            return null;
        }
    }

    private void ChangeAvatar(int avatarID){
        if(avatarID >= 0)
            currentAvatarID = avatarID % userAvatars.Count;
        else
            currentAvatarID = userAvatars.Count - 1;
        Debug.Log("AvatarID: " + currentAvatarID);
        avatarPH.GetComponent<Image>().sprite = userAvatars[currentAvatarID].GetComponent<Image>().sprite;
        avatarPH.GetComponent<Image>().material = userAvatars[currentAvatarID].GetComponent<Image>().material;
    }
    public void NextAvatar(){
        ChangeAvatar(currentAvatarID + 1);
    }
    public void PreviousAvatar(){
        ChangeAvatar(currentAvatarID - 1);
    }
}
