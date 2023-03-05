using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TotemEntities.DNA;
using TMPro;

public class AvatarList : MonoBehaviour
{
    //Content objct
    [SerializeField] private Transform avatarsParent;
    //Prefab for the avatars instantiated
    //[SerializeField] private GameObject avatarPrefab;
    List<GameObject> userAvatars = new List<GameObject>();
    public List<TotemDNADefaultAvatar> userAvatarsDNA = new List<TotemDNADefaultAvatar>();

    [Tooltip("Chtulk preview sprites")]
    [SerializeField] private List<Sprite> hairstyle = new List<Sprite>();
    [Tooltip("Sader for changin color")]
    [SerializeField] private Shader shader;
    [SerializeField] private GameObject avatarPH;
    [SerializeField] private TextMeshProUGUI avatarCount;
    private int currentAvatarID;

    public void BuildList(List<TotemDNADefaultAvatar> assets)
    {
        BuildDefaultAvatar();

        foreach (var asset in assets)
        {
            //Create one game object for each avatar
            GameObject currentAvatar = new GameObject("Avatar");
            RectTransform rectTransform = currentAvatar.AddComponent<RectTransform>();
            currentAvatar.transform.SetParent(avatarsParent.transform);
            currentAvatar.AddComponent<Image>();
            rectTransform = avatarPH.GetComponent<RectTransform>();
            
            //Set variables so it's shown correctly
            currentAvatar.GetComponent<Image>().sprite = SetHair(asset);
            Material mat = currentAvatar.GetComponent<Image>().material = new Material(shader);
            mat.SetColor("_BasePrimaryColour", asset.primary_color);
            mat.SetColor("_BaseEyeColour", asset.secondary_color);
            currentAvatar.SetActive(false);
            
            //add the go and the asset to the lists
            userAvatars.Add(currentAvatar);
            userAvatarsDNA.Add(asset);
        }
        //Set the default avatar
        ChangeAvatar(0);
    }
    private void BuildDefaultAvatar(){
        //Creating the game object
        GameObject defaultAvatar = new GameObject("Avatar");
            RectTransform defaultTransform = defaultAvatar.AddComponent<RectTransform>();
            defaultAvatar.transform.SetParent(avatarsParent.transform);
            defaultAvatar.AddComponent<Image>();
            defaultTransform = avatarPH.GetComponent<RectTransform>();

        //Defining default avatar asset variables
        TotemDNADefaultAvatar defaultAsset = new TotemDNADefaultAvatar();
            defaultAsset.hair_styles = "Short";
            defaultAsset.primary_color = new Color(138, 83, 11, 1);
            defaultAsset.secondary_color = new Color(34, 93, 100, 1);
            //Unused for now, defined just in case
            defaultAsset.sex_bio = false;
            defaultAsset.body_strength = false;
            defaultAsset.body_type = false;
            defaultAsset.human_eye_color = Color.white.ToString();
            defaultAsset.human_hair_color = Color.white.ToString();
            defaultAsset.human_skin_color = Color.white.ToString();
            
        //Tweaking the visible sprite
        defaultAvatar.GetComponent<Image>().sprite = SetHair(defaultAsset);
            Material defaultMaterial = defaultAvatar.GetComponent<Image>().material = new Material(shader);
            defaultMaterial.SetColor("_BasePrimaryColour", defaultAsset.primary_color);
            defaultMaterial.SetColor("_BaseEyeColour", defaultAsset.secondary_color);
            defaultAvatar.SetActive(false);

        //Adding it to the list so is the first option
        userAvatarsDNA.Add(defaultAsset);
        userAvatars.Add(defaultAvatar);
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
        avatarPH.GetComponent<Image>().sprite = userAvatars[currentAvatarID].GetComponent<Image>().sprite;
        avatarPH.GetComponent<Image>().material = userAvatars[currentAvatarID].GetComponent<Image>().material;
        changeNumber(currentAvatarID);
    }
    private void changeNumber(int number){
        if(number != 0 ){
            Debug.Log(number);
            avatarCount.text = number.ToString();
        }
        else{
            Debug.Log("fuck you " + number);
            avatarCount.text = "Default";
        }
    }
    public void NextAvatar(){
        ChangeAvatar(currentAvatarID + 1);
    }
    public void PreviousAvatar(){
        ChangeAvatar(currentAvatarID - 1);
    }


    public TotemDNADefaultAvatar getCurrentAvatar(){
        return userAvatarsDNA[currentAvatarID];
    }
    public Image getAvatarIcon(){
        return userAvatars[currentAvatarID].GetComponent<Image>();
    }
}
