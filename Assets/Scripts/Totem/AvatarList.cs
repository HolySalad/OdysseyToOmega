using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities.DNA;

public class AvatarList : MonoBehaviour
{
    //Content objct
    [SerializeField] private Transform avatarsParent;
    //Prefab for the avatars instantiated
    [SerializeField] private GameObject itemPrefab;

    public void BuildList(List<TotemDNADefaultAvatar> assets)
    {
        //clear existing items
        foreach (Transform avatar in avatarsParent)
        {
            Destroy(avatar.gameObject);
        }

        foreach (var asset in assets)
        {
            GameObject item = Instantiate(itemPrefab);
            item.transform.SetParent(avatarsParent);
            item.GetComponent<AvatarItem>().Setup(asset);
        }
    }

    public void ClearList()
        {
            foreach (Transform avatar in avatarsParent)
            {
                Destroy(avatar.gameObject);
            }
        }
}
