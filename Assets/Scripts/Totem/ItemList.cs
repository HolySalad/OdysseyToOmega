using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TotemEntities.DNA;
using TMPro;

public class ItemList : MonoBehaviour
{
    //Content objct
    [SerializeField] private Transform itemsParent;
    //Prefab for the avatars instantiated
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<ItemItem> itemObjects;
    [SerializeField] private List<GameObject> leftRightButtons;
    [SerializeField] private TextMeshProUGUI itemCount;

    [Header("Default colors")]
    [SerializeField] private Color defaultPrimaryColor = new Color(0, 152, 255, 1);
    [SerializeField] private Color defaultSecondaryColor = new Color(59, 252, 4, 1);
    private List<TotemDNADefaultItem> assets = new List<TotemDNADefaultItem>();
    private int currentItemIndex = 0;

    public void BuildList(List<TotemDNADefaultItem> assets)
    {
        this.assets.Add(BuildDefaultItem());
        foreach(TotemDNADefaultItem asset in assets){
            this.assets.Add(asset);
        }
        if (assets.Count == 0) {
            //TODO what happens if no assets?
        } else if (assets.Count == 1) {
            itemObjects[0].Hide();
            itemObjects[2].Hide();
            foreach (GameObject button in leftRightButtons) {
                button.SetActive(false);
            }
        }

        Debug.Log("ItemList: " + assets.Count);
        SetCurrentItemIndex(1);
        PreviousItem();
    }

    public TotemDNADefaultItem BuildDefaultItem() {
        TotemDNADefaultItem defaultItem = new TotemDNADefaultItem();
        defaultItem.classical_element = "Water";
        defaultItem.weapon_material = "Flint";
        defaultItem.primary_color = defaultPrimaryColor;
        defaultItem.secondary_color = defaultSecondaryColor;

        return defaultItem;
    }

    public void SetCurrentItemIndex(int index) {
        //Set the index of the current previous and next item
        currentItemIndex = index < 0 ? assets.Count-1 : index % assets.Count;
        int previousItemIndex = currentItemIndex == 0 ? assets.Count - 1 : currentItemIndex - 1;
        int nextItemIndex = currentItemIndex == assets.Count - 1 ? 0 : currentItemIndex + 1;

        ItemItem currentItem = itemObjects[1];
        ItemItem previousItem = itemObjects[0];
        ItemItem nextItem = itemObjects[2];

        previousItem.Setup(assets[previousItemIndex]);
        currentItem.Setup(assets[currentItemIndex]);
        changeNumber(currentItemIndex);
        //currentItem.ClickItem();
        nextItem.Setup(assets[nextItemIndex]);
    }

    private void changeNumber(int number){
        if(number != 0 ){
            itemCount.text = number.ToString();
        }
        else{
            itemCount.text = "Default";
        }
    }
        
    public void NextItem(){
        SetCurrentItemIndex(currentItemIndex + 1);
    }
    public void PreviousItem(){
        SetCurrentItemIndex(currentItemIndex - 1);
    }

    public TotemDNADefaultItem GetCurrentItem(){
        return itemObjects[currentItemIndex].thisAsset;
    }
}
