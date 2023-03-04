using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TotemEntities;
using TotemEntities.DNA;
using TotemServices.DNA;
using TMPro;

//TODO add default skin option in the avatars
//TODO confirm button that passes the selected skin and spear into a don't destroy on load object
//TODO update the avatar on the main menu when confirmed
//TODO main menu avatar (and frame) changing color when unsigned in
//TODO counter under avatar that shows the skin number

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
    public static TotemManager instance;
    private TotemCore totemCore;
    
    //I think we have to discuss the ID with the totem ppl
    public string _gameId = "OdisseyToO"; 

    //Avatars that the user has
    private List<TotemDNADefaultAvatar> _userAvatars;
    private List<TotemDNADefaultItem> _userItems;

    //Reference to the standard avatar, have to ask totem how to modify it
    private TotemDNADefaultAvatar firstAvatar;
    private TotemDNADefaultItem firstItem;

    //Events for when the player chooses any asset
    public delegate void ClickAvatar(string hairStyle, Color32 primaryColor, Color32 secondaryColor);
    public static event ClickAvatar OnClickedAvatar;
    public delegate void ClickItem(string material, string elememt, Color32 primaryColor, Color32 secondaryColor);
    public static event ClickItem OnClickedItem;

    [SerializeField] public GameObject frame;

    void Awake(){
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
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

            //avatarList.ClearList();

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

    public void callAvatarClicked(string hairstyle, Color32 primaryColor, Color32 secondaryColor){
        if(OnClickedAvatar != null)
            OnClickedAvatar(hairstyle,primaryColor,secondaryColor);
    }
    public void callItemClicked(string material, string element, Color32 primaryColor, Color32 secondaryColor){
        if(OnClickedItem != null)
            OnClickedItem(material, element,primaryColor,secondaryColor);
    }
}
