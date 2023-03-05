using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using TotemEntities;
using TotemEntities.DNA;
using TotemServices.DNA;
using TMPro;
using UnityEngine.UI;

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

    public GameObject frame;
    [SerializeField] private GameObject characterIcon;

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
        accountNameText.SetText(user.Name);
        assetsPanel.SetActive(true);
        loginPanel.SetActive(false);
        totemCore.GetUserAvatars<TotemDNADefaultAvatar>(user, TotemDNAFilter.DefaultAvatarFilter, (avatars) =>
        {
            //We get the avatars from the user
            _userAvatars = avatars;
            firstAvatar = avatars.Count > 0 ? avatars[0] : null;

            BuildAvatarList();

            //This was originally o nshowavatarrecords()
            loadingScreen.SetActive(false);
        });

        totemCore.GetUserItems<TotemDNADefaultItem>(user, TotemDNAFilter.DefaultItemFilter, (items) =>
            {
                //We get the items from the user
                _userItems = items;
                firstItem = items.Count > 0 ? items[0] : null;

                BuildItemList();

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

/*  This two are no longer necessary with the variable manager
    public void callAvatarClicked(string hairstyle, Color32 primaryColor, Color32 secondaryColor){
        if(OnClickedAvatar != null)
            OnClickedAvatar(hairstyle,primaryColor,secondaryColor);
    }
    public void callItemClicked(string material, string element, Color32 primaryColor, Color32 secondaryColor){
        if(OnClickedItem != null)
            OnClickedItem(material, element,primaryColor,secondaryColor);
    }
*/

    public void confirmButton(){
        VariableManager.Instance.avatar = avatarList.getCurrentAvatar();
        characterIcon.GetComponent<Image>().sprite = avatarList.getAvatarIcon().sprite;
        characterIcon.GetComponent<Image>().material = avatarList.getAvatarIcon().material;
        if(characterIcon.GetComponent<TwistingColours>()){
            Destroy(characterIcon.GetComponent<TwistingColours>());
        }
        VariableManager.Instance.harpoon = itemList.GetCurrentItem();
    }
}
