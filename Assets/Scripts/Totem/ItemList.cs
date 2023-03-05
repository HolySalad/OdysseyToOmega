using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TotemEntities.DNA;

public class ItemList : MonoBehaviour
{
    //Content objct
    [SerializeField] private Transform itemsParent;
    //Prefab for the avatars instantiated
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private List<ItemItem> itemObjects;
    [SerializeField] private List<GameObject> leftRightButtons;
    private List<TotemDNADefaultItem> assets = new List<TotemDNADefaultItem>();
    private int currentItemIndex = 0;

    public void BuildList(List<TotemDNADefaultItem> assets)
    {
        this.assets = assets;
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
    }

    public void ClearList() {
        //no
    }

    public void SetCurrentItemIndex(int index) {
        currentItemIndex = index < 0 ? assets.Count-1 : index % assets.Count;
        int previousItemIndex = currentItemIndex == 0 ? assets.Count - 1 : currentItemIndex - 1;
        int nextItemIndex = currentItemIndex == assets.Count - 1 ? 0 : currentItemIndex + 1;
        ItemItem currentItem = itemObjects[1];
        ItemItem previousItem = itemObjects[0];
        ItemItem nextItem = itemObjects[2];

        previousItem.Setup(assets[previousItemIndex]);
        currentItem.Setup(assets[currentItemIndex]);
        currentItem.ClickItem();
        nextItem.Setup(assets[nextItemIndex]);
    }
        
    public void NextItem(){
        SetCurrentItemIndex(currentItemIndex + 1);
    }
    public void PreviousItem(){
        SetCurrentItemIndex(currentItemIndex - 1);
    }
}
