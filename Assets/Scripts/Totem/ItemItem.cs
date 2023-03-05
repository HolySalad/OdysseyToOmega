using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities.DNA;
using TMPro;
using UnityEngine.UI;

public class ItemItem : MonoBehaviour
{
    //This goes on the avatar item prefab to initialize it
    private TotemDNADefaultItem asset;

    private GameObject material;
    private GameObject elememt;

    [SerializeField] List<Sprite> elementType = new List<Sprite>();
    [SerializeField] List<Sprite> materialType = new List<Sprite>();

    [SerializeField] private Shader shader;

    private TotemDNADefaultItem thisAsset;

    private bool thisClicked = false;

    public void Setup(TotemDNADefaultItem asset)
    {
        thisAsset = asset;

        TotemManager.OnClickedItem += deleteSelection;

        material = transform.Find("Material").gameObject;
        elememt = transform.Find("Element").gameObject;

        switch(asset.classical_element){
            case ("Air"): elememt.GetComponent<Image>().sprite = elementType[3];
            break;
            case ("Earth"): elememt.GetComponent<Image>().sprite = elementType[0];
            break;
            case ("Water"): elememt.GetComponent<Image>().sprite = elementType[1];
            break;
            case ("Fire"): elememt.GetComponent<Image>().sprite = elementType[2];
            break;
            default: elememt.GetComponent<Image>().sprite = elementType[0];
            break;
        }

        switch(asset.weapon_material){
            case ("Wood"): material.GetComponent<Image>().sprite = materialType[0];
            break;
            case ("Bone"): material.GetComponent<Image>().sprite = materialType[1];
            break;
            case ("Flint"): material.GetComponent<Image>().sprite = materialType[2];
            break;
            case ("Obsidian"): material.GetComponent<Image>().sprite = materialType[3];
            break;
            default: material.GetComponent<Image>().sprite = materialType[0];
            break;
        }

        Material matMat = material.GetComponent<Image>().material = new Material(shader);
        matMat.SetColor("_BaseEyeColour", asset.primary_color);
        Material eleMat = elememt.GetComponent<Image>().material = new Material(shader);
        eleMat.SetColor("_BasePrimaryColour", asset.secondary_color);

        //Operations to initialize the prefab
        //Asset gives info about the caracteristics
        Debug.Log("Element: " + asset.classical_element);
        Debug.Log("Material: " + asset.weapon_material);
        Debug.Log("PrimaryColor: " + asset.primary_color);
        Debug.Log("SecondaryColor: " + asset.secondary_color);

        //I have no idea what would you use this for
        this.asset = asset;
    }
    void Start(){
        //transform.localScale = new Vector3(1,1,1);
    }


    public void Hide() {
        material = transform.Find("Material").gameObject;
        elememt = transform.Find("Element").gameObject;

        material.GetComponent<Image>().enabled = false;
        elememt.GetComponent<Image>().enabled = false;
    }
    

    private void deleteSelection(string a, string a2, Color32 b, Color32 c){
        if(thisClicked)
            thisClicked = false;
        else{
            if(material.GetComponent<Transform>().Find("Frame(Clone)") != null)
                Destroy(material.GetComponent<Transform>().Find("Frame(Clone)").gameObject);
        }
    }

    public void ClickItem(){
        thisClicked = true;
        if(material.GetComponent<Transform>().Find("Frame(Clone)") == null)
            Instantiate(TotemManager.instance.frame, transform.position, transform.rotation, material.GetComponent<Transform>());
        TotemManager.instance.callItemClicked(thisAsset.weapon_material, thisAsset.classical_element, thisAsset.primary_color, thisAsset.secondary_color);
    }
}
