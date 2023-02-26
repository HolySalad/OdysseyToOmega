using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities.DNA;

public class ItemList : MonoBehaviour
{
    //Content objct
    [SerializeField] private Transform itemsParent;
    //Prefab for the avatars instantiated
    [SerializeField] private GameObject itemPrefab;

    public void BuildList(List<TotemDNADefaultItem> assets)
    {
        //clear existing items
        foreach (Transform item in itemsParent)
        {
            Destroy(item.gameObject);
        }

        foreach (var asset in assets)
        {
            GameObject item = Instantiate(itemPrefab);
            item.transform.SetParent(itemsParent);
            item.GetComponent<ItemItem>().Setup(asset);
        }
    }

    public void ClearList()
        {
            foreach (Transform item in itemsParent)
            {
                Destroy(item.gameObject);
            }
        }
}
