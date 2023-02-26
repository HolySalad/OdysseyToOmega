using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TotemEntities;
using TotemEntities.DNA;
using TotemServices.DNA;
using TMPro;
using SpaceBoat;

namespace TotemDemo
{


    public class TotemManager : MonoBehaviour
    {
        enum Avatarss
        {
            Afro,
            asymmetrical,
            braids,
            buzzcut,
            dreadlocks,
            longg,
            ponytail,
        }


        public static TotemManager Instance;
        private TotemCore totemCore;

        /// <summary>
        /// Id of your game, used for legacy records identification. 
        /// Note, that if you are targeting mobile platforms you also have to use this id for deepLink generation in
        /// *Window > Totem Generator > Generate Deep Link* menu
        /// </summary>
        [Header("Demo")]
        public string _gameId = "TotemDemo";
        [SerializeField] private GameObject loginPanel;
        [SerializeField] private GameObject selectionPanel;
        [SerializeField] private GameObject loginButton;

        [Header("Login UI")]
        [SerializeField] private GameObject googleLoginObject;
        [SerializeField] private GameObject profileNameObject;
        [SerializeField] private TextMeshProUGUI profileNameText;
        [SerializeField] private TMP_Dropdown dropdown;
        Dictionary<string, Avatarss> avatarssDictionary = new Dictionary<string, Avatarss>();
        Dictionary<Avatarss,SpriteRenderer> avatar_to_spriteRenderer = new Dictionary<Avatarss,SpriteRenderer>();

        [Header("Legacy UI")]
        [SerializeField] private TMP_InputField legacyGameIdInput;
        [SerializeField] private TMP_InputField dataToCompoareInput;
        [SerializeField] private Animator popupAnimator;
        [SerializeField] List<TMP_Dropdown.OptionData> optionList;

        //Meta Data
        private TotemUser _currentUser;
        private List<TotemDNADefaultAvatar> _userAvatars;
        [SerializeField] private Sprite[] avatarSprites = new Sprite[7];
        [SerializeField] private SpriteRenderer[] avatarSpriteRenderer = new SpriteRenderer[7];
        //Default Avatar reference - use for your game
        private TotemDNADefaultAvatar firstAvatar;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.L))
            {
                if(!loginPanel.activeInHierarchy)
                {

                GameModel.Instance.PauseGame();
                loginPanel.SetActive(true);
                }
                else
                {
                    GameModel.Instance.UnpauseGame();
                    loginPanel.SetActive(false);
                }
            }    


            if (_userAvatars != null)
            {

                foreach (TotemDNADefaultAvatar avatar in _userAvatars)
                {
                    Debug.Log(avatarssDictionary[avatar.hair_styles]);
                    Debug.Log(dropdown.options[dropdown.value].text);

                    if (avatar.ToString() == dropdown.options[dropdown.value].text)
                    {
                        GameModel.Instance.SetAvatar(avatar);
                        foreach(SpriteRenderer spriteRenderer in avatarSpriteRenderer)
                        {
                            spriteRenderer.enabled = false;
                        }
                        avatar_to_spriteRenderer[avatarssDictionary[avatar.hair_styles]].enabled = true;
                    }

                }
            }
        }
        /// <summary>
        /// Initializing TotemCore
        /// </summary>
        void Start()
        {
            totemCore = new TotemCore(_gameId);


            avatarssDictionary.Add("afro", Avatarss.Afro);
            avatarssDictionary.Add("asymmetrical", Avatarss.asymmetrical);
            avatarssDictionary.Add("braids", Avatarss.braids);
            avatarssDictionary.Add("buzz cut", Avatarss.buzzcut);
            avatarssDictionary.Add("dreadlocks", Avatarss.dreadlocks);
            avatarssDictionary.Add("long", Avatarss.longg);
            avatarssDictionary.Add("ponytail", Avatarss.ponytail);
            avatarssDictionary.Add("short", Avatarss.Afro);


            avatar_to_spriteRenderer.Add(Avatarss.Afro, avatarSpriteRenderer[0]);
            avatar_to_spriteRenderer.Add(Avatarss.asymmetrical, avatarSpriteRenderer[1]);
            avatar_to_spriteRenderer.Add(Avatarss.braids, avatarSpriteRenderer[2]);
            avatar_to_spriteRenderer.Add(Avatarss.buzzcut, avatarSpriteRenderer[3]);
            avatar_to_spriteRenderer.Add(Avatarss.dreadlocks, avatarSpriteRenderer[4]);
            avatar_to_spriteRenderer.Add(Avatarss.longg, avatarSpriteRenderer[5]);
            avatar_to_spriteRenderer.Add(Avatarss.ponytail, avatarSpriteRenderer[6]);

            // legacyGameIdInput.onEndEdit.AddListener(OnGameIdInputEndEdit);
        }


        #region USER AUTHENTICATION
        public void OnLoginButtonClick()
        {

            //Login user
            totemCore.AuthenticateCurrentUser(OnUserLoggedIn);
        }

        private void OnUserLoggedIn(TotemUser user)
        {
            //Using default filter with a default avatar model. You can implement your own filters and/or models
            totemCore.GetUserAvatars<TotemDNADefaultAvatar>(user, TotemDNAFilter.DefaultAvatarFilter, (avatars) =>
            {
                //googleLoginObject.SetActive(false);
                profileNameObject.SetActive(true);
                profileNameText.SetText(user.Name);
                selectionPanel.SetActive(true);
                
                //Avatars
                _userAvatars = avatars;
                firstAvatar = avatars.Count > 0 ? avatars[0] : null;
                //
                //BuildAvatarList();

                //
                foreach (var avatar in avatars)
                {
                    Debug.Log(avatarssDictionary[avatar.hair_styles]);

                }
                //UI Example Methods
                AddToDropDown();
                //ShowAvatarRecords();


            });

            totemCore.GetUserItems<TotemDNADefaultItem>(user, TotemDNAFilter.DefaultItemFilter, (items) =>
            {
                Debug.Log("Items:");
                foreach (var item in items)
                {
                    Debug.Log(item.ToString());
                }
            });
        }


        //public void ShowAvatarRecords()
        //{
        //    GetLegacyRecords(firstAvatar, TotemAssetType.avatar, (records) =>
        //    {
                
        //    });
        //}
        #endregion

        #region LEGACY RECORDS
        /// <summary>
        /// Add a new Legacy Record to a specific Totem Asset.
        /// </summary>
        //public void AddLegacyRecord(object asset, TotemAssetType assetType, int data)
        //{
        //    UILoadingScreen.Instance.Show();
        //    totemCore.AddLegacyRecord(asset, assetType, data.ToString(), (record) =>
        //    {
        //        legacyRecordsList.AddRecordToList(record, true);
        //        UILoadingScreen.Instance.Hide();
        //        popupAnimator.Play("Write Legacy");
        //    });
        //}

        /// <summary>
        /// Add a new Legacy Record to the first Totem Avatar.
        /// </summary>
        //public void AddLegacyToFirstAvatar(int data)
        //{
        //    AddLegacyRecord(firstAvatar, TotemAssetType.avatar, data);
        //}

        //public void GetLegacyRecords(object asset, TotemAssetType assetType, UnityAction<List<TotemLegacyRecord>> onSuccess)
        //{
        //    totemCore.GetLegacyRecords(asset, assetType, onSuccess, string.IsNullOrEmpty(legacyGameIdInput.text) ? _gameId : legacyGameIdInput.text);
        //}

        //public void GetLastLegacyRecord(UnityAction<TotemLegacyRecord> onSuccess)
        //{
        //    GetLegacyRecords(firstAvatar, TotemAssetType.avatar, (records) => { onSuccess.Invoke(records[records.Count - 1]); });
        //}

        //public void CompareLastLegacyRecord()
        //{
        //    GetLastLegacyRecord((record) =>
        //    {
        //        string valueToCheckText = dataToCompoareInput.text;
        //        if (valueToCheckText.Equals(record.data))
        //        {
        //            popupAnimator.Play("Read Legacy");
        //        }
        //    }
        //    );
        //}
        #endregion

        #region UI EXAMPLE METHOD

    

        private void AddToDropDown()
        {
            foreach (TotemDNADefaultAvatar avatar in _userAvatars)
            {
                if (avatar != null)
                {

                    TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData();
                    data.text = avatar.ToString();
                    data.image = avatarSprites[(int)avatarssDictionary[avatar.hair_styles]];


                    optionList.Add(data);
                }
            }
            dropdown.ClearOptions();
            dropdown.AddOptions(optionList);


        }

        //private void OnGameIdInputEndEdit(string text)
        //{
        //    ShowAvatarRecords();
        //}

        #endregion
    }
}