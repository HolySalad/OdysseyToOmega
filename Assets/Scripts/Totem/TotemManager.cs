using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TotemEntities;
using TotemEntities.DNA;
using TotemServices.DNA;
using TMPro;


public class TotemManager : MonoBehaviour
{
    //Objcts from scene
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject assetsPanel;
    [SerializeField] private AvatarList avatarList;
    [SerializeField] private ItemList itemList;
    [SerializeField] private TextMeshProUGUI accountNameText;

    //Classes for totem
    public static TotemManager Instance;
    private TotemCore totemCore;
    
    //I think we have to discuss the ID with the totem ppl
    public string _gameId = "OdisseyToO"; 

    //Avatars that the user has
    private List<TotemDNADefaultAvatar> _userAvatars;
    private List<TotemDNADefaultItem> _userItems;

    //Reference to the standard avatar, have to ask totem how to modify it
    private TotemDNADefaultAvatar firstAvatar;
    private TotemDNADefaultItem firstItem;

    void Awake(){
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start(){
        totemCore = new TotemCore(_gameId);

        //Try to log in with the last user
        totemCore.AuthenticateLastUser(OnUserLoggedIn, (error) =>
        {
            loadingScreen.SetActive(false);
        });
    }

    public void OnLoginButtonClick()
    {
        loadingScreen.SetActive(true);

        //Login user
        totemCore.AuthenticateCurrentUser(OnUserLoggedIn);
    }

    private void OnUserLoggedIn(TotemUser user)
    {
        totemCore.GetUserAvatars<TotemDNADefaultAvatar>(user, TotemDNAFilter.DefaultAvatarFilter, (avatars) =>
        {
            Debug.Log("Avatars:");
            //Aqui hay que quitar el botón y meter el nombre de usuario
            accountNameText.SetText(user.Name);
            assetsPanel.SetActive(true);
            loginPanel.SetActive(false);

            avatarList.ClearList();

            //We get the avatars from the user
            _userAvatars = avatars;
            firstAvatar = avatars.Count > 0 ? avatars[0] : null;

            //Example to implement
            BuildAvatarList();
            //Idk what is this, probably no necesary??
            //ShowAvatarRecords();

            //This was originally o nshowavatarrecords()
            loadingScreen.SetActive(false);
        });

        totemCore.GetUserItems<TotemDNADefaultItem>(user, TotemDNAFilter.DefaultItemFilter, (items) =>
            {
                Debug.Log("Items:");
                itemList.ClearList();

                //We get the avatars from the user
                _userItems = items;
                firstItem = items.Count > 0 ? items[0] : null;

                //Example to implement
                BuildItemList();
                //Idk what is this, probably no necesary??
                //ShowAvatarRecords();

                //This was originally o nshowavatarrecords()
                loadingScreen.SetActive(false);
            });
    }

    private void BuildAvatarList()
    {
        avatarList.BuildList(_userAvatars);
    }
    private void BuildItemList()
    {
        itemList.BuildList(_userItems);
    }
}
