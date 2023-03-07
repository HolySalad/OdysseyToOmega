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
    [SerializeField] private TextMeshProUGUI promoText;
    [SerializeField] private string LoginPromo = "";
    [SerializeField] private string LoginErrorApology = "";
    [SerializeField] private bool LoginByDefault = true;

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
        VariableManager.Instance.Avatar = avatarList.GetDefaultAsset();
        promoText.text = LoginPromo;
        //Try to log in with the last user
        if (LoginByDefault) totemCore.AuthenticateLastUser(OnUserLoggedIn, (error) =>
        {
            loadingScreen.SetActive(false);
        });
    }

    public void OnLoginButtonClick()
    {
        loadingScreen.SetActive(true);
        loginPanel.SetActive(false);

        //Login user
        totemCore.AuthenticateCurrentUser(OnUserLoggedIn, (error) =>
        {
            loadingScreen.SetActive(false);
            loginPanel.SetActive(true);
            promoText.text = LoginErrorApology;
        });
    }

    private void OnUserLoggedIn(TotemUser user)
    {
        Debug.Log("Totem user logged in: " + user.Name + " - " + user.Email + " - " + user.PublicKey);
        if(characterIcon.GetComponent<TwistingColours>()){
            characterIcon.GetComponent<TwistingColours>().StopTwist(new Color(0.86f, 0.69f, 0.32f));
        }
        VariableManager.Instance.SetTotemUser(user.PublicKey);
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
            VariableManager.Instance.Avatar = avatarList.getCurrentAvatar();
            VariableManager.Instance.DefaultAvatar = avatarList.isDefault;
        });

        totemCore.GetUserItems<TotemDNADefaultItem>(user, TotemDNAFilter.DefaultItemFilter, (items) =>
            {
                //We get the items from the user
                _userItems = items;
                firstItem = items.Count > 0 ? items[0] : null;

                BuildItemList();

                //This was originally o nshowavatarrecords()
                loadingScreen.SetActive(false);
                VariableManager.Instance.Harpoon = itemList.GetCurrentItem();
                VariableManager.Instance.DefaultHarpoon = itemList.isDefault;
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
        VariableManager.Instance.Avatar = avatarList.getCurrentAvatar();
        VariableManager.Instance.DefaultAvatar = avatarList.isDefault;
        VariableManager.Instance.Harpoon = itemList.GetCurrentItem();
        VariableManager.Instance.DefaultHarpoon = itemList.isDefault;
        VariableManager.Instance.SaveAvatars();
    }
}
