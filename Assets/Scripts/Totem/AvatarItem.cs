using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities.DNA;
using TMPro;
using UnityEngine.UI;
public class AvatarItem : MonoBehaviour
{
    //This goes on the avatar item prefab to initialize it
    private TotemDNADefaultAvatar asset;
    [SerializeField] private List<Sprite> hairstyle = new List<Sprite>();
    [SerializeField] private Shader shader;

    private TotemDNADefaultAvatar thisAsset;

    private bool thisClicked = false;
    public void Setup(TotemDNADefaultAvatar asset)
    {
        thisAsset = asset;

        TotemManager.OnClickedAvatar += deleteSelection;

        if(hairstyle[0] != null){
                Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" + asset.hair_styles);
            switch(asset.hair_styles){
                case "asymmetrical":
                    GetComponent<Image>().sprite = hairstyle[0];
                break;
                case "braids":
                    GetComponent<Image>().sprite = hairstyle[1];
                break;
                case "buzzCut":
                    GetComponent<Image>().sprite = hairstyle[2];
                break;
                case "dreadlocks":
                    GetComponent<Image>().sprite = hairstyle[3];
                break;
                case "long":
                    GetComponent<Image>().sprite = hairstyle[4];
                break;
                case "ponytail":
                    GetComponent<Image>().sprite = hairstyle[5];
                break;
                case "short":
                    GetComponent<Image>().sprite = hairstyle[6];
                break;
                default:
                    GetComponent<Image>().sprite = hairstyle[6];
                break;
            }
        }

        Material mat = GetComponent<Image>().material = new Material(shader);
        mat.SetColor("_BasePrimaryColour", asset.primary_color);
        mat.SetColor("_BaseEyeColour", asset.secondary_color);
        //Operations to initialize the prefab
        //Asset gives info about the caracteristics
        Debug.Log("Hair style: " + asset.hair_styles);
        Debug.Log("PrimaryColor: " + asset.primary_color);
        Debug.Log("SecondaryColor: " + asset.secondary_color);

        //I have no idea what would you use this for
        this.asset = asset;
    }

    private void deleteSelection(string a, Color32 b, Color32 c){
        if(thisClicked)
            thisClicked = false;
        else{
            if(transform.Find("Frame(Clone)") != null)
                Destroy(transform.Find("Frame(Clone)").gameObject);
        }
    }

    public void ClickAvatar(){
        thisClicked = true;
        if(transform.Find("Frame(Clone)") == null)
            Instantiate(TotemManager.instance.frame, transform.position, transform.rotation, transform);
        TotemManager.instance.callAvatarClicked(thisAsset.hair_styles, thisAsset.primary_color, thisAsset.secondary_color);
    }
}
