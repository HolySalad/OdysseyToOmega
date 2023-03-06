using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Rewards;
using SpaceBoat.PlayerSubclasses.Equipment;
using UnityEngine.UI;
using TMPro;

namespace SpaceBoat.UI {
    public enum CraftUIState {
        EquipmentPanel,
        StorePanel,
        //TotemPanel 
    }
    public class CraftingUI : MonoBehaviour
    {
        [SerializeField] private GameObject equipmentPanel;
        [SerializeField] private GameObject storePanel;
        //[SerializeField] private GameObject totemPanel;

        [Header("Store Panel")]
        [SerializeField] private GameObject MakeASelectionText;
        [SerializeField] private GameObject TemplateStoreItem;
        [SerializeField] private GameObject StoreContentPanel;
        [SerializeField] private GameObject StoreContentBox;
        [SerializeField] private GameObject StoreDetailsPanel;
        [SerializeField] private GameObject StoreDetailsPanelParent;
        [SerializeField] private GameObject StoreNoAvailableItemsPanel;
        [SerializeField] private GameObject StoreNoAvailableItemsText;
        [SerializeField] private TextMeshProUGUI StoreDetailsTitle;
        [SerializeField] private TextMeshProUGUI StoreDetailsSubtitle;
        [SerializeField] private TextMeshProUGUI StoreDetailsDescription;
        [SerializeField] private TextMeshProUGUI StoreDetailsFurtherDescription;
        [SerializeField] private TextMeshProUGUI StoreDetailsCost;
        [SerializeField] private Image StoreDetailsImage;
        [SerializeField] private Button StoreCraftButton;
        [SerializeField] private string StoreNothingUnlockedText = "You haven't found any blueprints. Destroy Comets with the Harpoon Launcher to find new blueprints.";
        [SerializeField] private string StoreNothingNotYetBuiltText = "You have crafted all of the available blueprints. Destroy Comets with the Harpoon Launcher to find more blueprints.";

        [Header("Equipment Panel")]
        [SerializeField] private TextMeshProUGUI equipmentTitleText;
        [SerializeField] private TextMeshProUGUI equipmentDescriptionText;
        [SerializeField] private TextMeshProUGUI equipmentFurtherDescriptionText;
        [SerializeField] private string NoEquipmentText = "No Equipment";
        [SerializeField] private GameObject dashEquipmentButtonObject;
        [SerializeField] private Image dashEquipmentButtonBackground;
        [SerializeField] private GameObject healthPackEquipmentButtonObject;
        [SerializeField] private Image healthPackEquipmentButtonBackground;
        [SerializeField] private GameObject shieldEquipmentButtonObject;
        [SerializeField] private Image shieldEquipmentButtonBackground;

        public CraftUIState CurrentState { get; private set; }

        private int currentPanelIndex = 1;
        private CraftUIState[] panels = new CraftUIState[] { CraftUIState.EquipmentPanel, CraftUIState.StorePanel/*, CraftUIState.TotemPanel */};

        private Dictionary<RewardType, ICraftBlueprint> blueprints = new Dictionary<RewardType, ICraftBlueprint>();
        private Dictionary<EquipmentType, ICraftBlueprint> equipmentBlueprints = new Dictionary<EquipmentType, ICraftBlueprint>();
        private ICraftBlueprint selectedBlueprint;

        private EquipmentType[] equipmentTypes = new EquipmentType[] { EquipmentType.Dash, EquipmentType.HealthPack, EquipmentType.Shield };
        private int numEquipmentOwned = 0;
        private EquipmentType pendingEquipmentType = EquipmentType.None;

        void Awake() {
            blueprints[RewardType.DashEquipmentBlueprint] = GetComponent<DashEquipmentBlueprint>();
            blueprints[RewardType.HealthPackEquipmentBlueprint] = GetComponent<HealthPackEquipmentBlueprint>();
            blueprints[RewardType.ShieldEquipmentBlueprint] = GetComponent<ShieldEquipmentBlueprint>();
            blueprints[RewardType.JumpPadBuildableBlueprint] = GetComponent<JumpPadBuildableBlueprint>();
            blueprints[RewardType.ShipShieldBuildableBlueprint] = GetComponent<ShipShieldBuildableBlueprint>();

            equipmentBlueprints[EquipmentType.Dash] = GetComponent<DashEquipmentBlueprint>();
            equipmentBlueprints[EquipmentType.HealthPack] = GetComponent<HealthPackEquipmentBlueprint>();
            equipmentBlueprints[EquipmentType.Shield] = GetComponent<ShieldEquipmentBlueprint>();
        }

        void Start() {
            TotemManager totemManager = FindObjectOfType<TotemManager>();
            TotemManager.ClickAvatar avatarClick = (string hairStyle, Color32 primaryColour, Color32 secondaryColour) => { 
                Debug.Log("Pending Avatar " + hairStyle + " " + primaryColour + " " + secondaryColour);
                pendingTotemCthulk = new PendingTotemCthulk(hairStyle, primaryColour, secondaryColour);
                hasPendingTotemCthulk = true;
            };
            TotemManager.OnClickedAvatar +=  avatarClick;

            TotemManager.ClickItem itemClick = (string material, string element, Color32 primaryColour, Color32 secondaryColour) => { 
                Debug.Log("Pending Item " + material + " " + element + " " + primaryColour + " " + secondaryColour);
                pendingTotemHarpoon = new PendingTotemHarpoon( material,  element,  primaryColour,  secondaryColour);
                hasPendingTotemHarpoon = true;
            };
            TotemManager.OnClickedItem += itemClick;
        }

        void ChangePanel(CraftUIState state) {
            CurrentState = state;
            switch (state) {
                case CraftUIState.EquipmentPanel:
                    equipmentPanel.SetActive(true);
                    storePanel.SetActive(false);
                    //totemPanel.SetActive(false);
                    OpenEquipmentPanel();
                    break;
                case CraftUIState.StorePanel:
                    equipmentPanel.SetActive(false);
                    storePanel.SetActive(true);
                    //totemPanel.SetActive(false);
                    ClearStorePanelDetails();
                    CreateCraftingOptions();
                    break;
                    /*
                case CraftUIState.TotemPanel:
                    equipmentPanel.SetActive(false);
                    storePanel.SetActive(false);
                    totemPanel.SetActive(true);
                    break;
                    */
                default:
                    Debug.LogWarning("CraftingUI.cs: ChangePanel() switch statement reached default case.");
                    break;
            }
        }

        //store panel

        void SetStorePanelDetails(RewardType rewardType) {
            if (blueprints.ContainsKey(rewardType)) {
                selectedBlueprint = blueprints[rewardType];
                MakeASelectionText.SetActive(false);
                StoreDetailsTitle.text = UIManager.Instance.FixedUIText(selectedBlueprint.Title);
                StoreDetailsSubtitle.text = UIManager.Instance.FixedUIText(selectedBlueprint.Subtitle);
                StoreDetailsDescription.text = UIManager.Instance.FixedUIText(selectedBlueprint.Description);
                StoreDetailsFurtherDescription.text = UIManager.Instance.FixedUIText(selectedBlueprint.FurtherDescription);
                StoreDetailsCost.text = UIManager.Instance.FixedUIText(selectedBlueprint.Cost.ToString());
                StoreDetailsImage.sprite = selectedBlueprint.IconLarge;

                StoreCraftButton.interactable = !selectedBlueprint.AlreadyOwns(GameModel.Instance.player) && GameModel.Instance.player.PlayerHasMoney(selectedBlueprint.Cost);
                StoreDetailsPanel.SetActive(true);
            }
        }

        void ClearStorePanelDetails() {
            selectedBlueprint = null;
            MakeASelectionText.SetActive(true);
            StoreDetailsTitle.text = UIManager.Instance.FixedUIText("");
            StoreDetailsSubtitle.text = UIManager.Instance.FixedUIText("");
            StoreDetailsDescription.text = UIManager.Instance.FixedUIText("");
            StoreDetailsFurtherDescription.text = UIManager.Instance.FixedUIText("");
            StoreDetailsCost.text = UIManager.Instance.FixedUIText("");
            StoreDetailsImage.sprite = null;
            StoreCraftButton.interactable = false;
            StoreDetailsPanel.SetActive(false);
            StoreNoAvailableItemsPanel.SetActive(false);
        }

        public void StoreCraftingButtonPressed() {
            if (selectedBlueprint != null) {
                Debug.Log("CraftingUI.cs: StoreCraftingButtonPressed() selectedBlueprint == " + selectedBlueprint.Title);
                selectedBlueprint.Craft(GameModel.Instance.player);
                SoundManager.Instance.Play("ItemCrafted");
                switch (selectedBlueprint.BlueprintType) {
                    case BlueprintType.Equipment:
                        if (numEquipmentOwned == 0) {
                            pendingEquipmentType = GameModel.Instance.player.lastCraftedEquipmentType;
                            UIManager.Instance.CloseCraftingMenu();
                        } else {
                            currentPanelIndex = (int)CraftUIState.EquipmentPanel;
                            ChangePanel(panels[currentPanelIndex]);
                        }
                        break;
                    case BlueprintType.Buildable:
                        break;
                    default:
                        Debug.LogWarning("CraftingUI.cs: StoreCraftingButtonPressed() switch statement reached default case.");
                        break;
                }
            } else {
                Debug.LogWarning("CraftingUI.cs: StoreCraftingButtonPressed() selectedBlueprint is null.");
            }
        }

        void CreateCraftingOptions() {
            if (StoreContentBox.transform.childCount > 1) {
                for (int i = 1; i < StoreContentBox.transform.childCount; i++) {
                    Destroy(StoreContentBox.transform.GetChild(i).gameObject);
                }
            }
            int numButtons = 0;
            int numBlueprintsOwned = 0;
            bool ownsAtLeastOne = false;
            foreach (ICraftBlueprint blueprint in blueprints.Values) {
                bool alreadyOwns = blueprint.AlreadyOwns(GameModel.Instance.player);
                bool rewardUnlocked = GameModel.Instance.unlockEverything || GameModel.Instance.saveGame.rewardsUnlocked[blueprint.RewardType];
                
                    Debug.Log("store item for " + blueprint.RewardType + " already owns: " + alreadyOwns + " reward unlocked: " + rewardUnlocked);
                if (!alreadyOwns && rewardUnlocked) {
                    Debug.Log("Creating store item for " + blueprint.RewardType);
                    numBlueprintsOwned++;
                    GameObject storeItem = Instantiate(TemplateStoreItem, StoreContentBox.transform);
                    RectTransform rect = storeItem.GetComponent<RectTransform>();
//                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - (numButtons * 120));
                    numButtons++;
                    storeItem.GetComponent<Button>().onClick.AddListener(() => SetStorePanelDetails(blueprint.RewardType));
                    storeItem.transform.Find("Image").gameObject.GetComponent<Image>().sprite = blueprint.IconSmall;
                    storeItem.transform.Find("Cost").gameObject.GetComponent<TextMeshProUGUI>().text = UIManager.Instance.FixedUIText(blueprint.Cost.ToString());
                    storeItem.SetActive(true);
                } else if (alreadyOwns) {
                    ownsAtLeastOne = true;
                }
            }
            TemplateStoreItem.SetActive(false);
            if (numBlueprintsOwned == 0) {
                StoreDetailsPanelParent.SetActive(false);
                StoreContentPanel.SetActive(false);
                StoreNoAvailableItemsPanel.SetActive(true);
                if (ownsAtLeastOne) {
                    StoreNoAvailableItemsText.GetComponent<TextMeshProUGUI>().text = UIManager.Instance.FixedUIText(StoreNothingNotYetBuiltText);
                } else {
                    StoreNoAvailableItemsText.GetComponent<TextMeshProUGUI>().text = UIManager.Instance.FixedUIText(StoreNothingUnlockedText);
                }
            } else {
                StoreDetailsPanelParent.SetActive(true);
                StoreContentPanel.SetActive(true);
                StoreNoAvailableItemsPanel.SetActive(false);
            }
        }

        //equipment panel

        
        void SetEquipmentPanelDetails(EquipmentType type) {
            if (type == EquipmentType.None) {
                equipmentDescriptionText.text = UIManager.Instance.FixedUIText(NoEquipmentText);
                equipmentFurtherDescriptionText.text = UIManager.Instance.FixedUIText("");
                equipmentTitleText.text = "";
                return;
            }
            equipmentTitleText.text = UIManager.Instance.FixedUIText(equipmentBlueprints[type].Title);
            equipmentDescriptionText.text = UIManager.Instance.FixedUIText(equipmentBlueprints[type].Description);
            equipmentFurtherDescriptionText.text = UIManager.Instance.FixedUIText(equipmentBlueprints[type].FurtherDescription);
        }
        public void SelectDash() {
            SetEquipmentPanelDetails(EquipmentType.Dash);
            pendingEquipmentType = EquipmentType.Dash;
            SetupEquipmentButtons();
        }

        public void SelectHealthPack() {
            SetEquipmentPanelDetails(EquipmentType.HealthPack);
            pendingEquipmentType = EquipmentType.HealthPack;
            SetupEquipmentButtons();
        }

        public void SelectShield() {
            SetEquipmentPanelDetails(EquipmentType.Shield);
            pendingEquipmentType = EquipmentType.Shield;
            SetupEquipmentButtons();
        }

        void SetupEquipmentButtons() {
            Button dashButton = dashEquipmentButtonObject.GetComponent<Button>();
            Image dashImage = dashButton.GetComponent<Image>();
            dashImage.sprite = equipmentBlueprints[EquipmentType.Dash].IconLarge;
            dashEquipmentButtonBackground.sprite = equipmentBlueprints[EquipmentType.Dash].IconLarge;
            Player player = GameModel.Instance.player;
            Debug.Log("player has dash: " + player.HasEquipment(EquipmentType.Dash));
            if (player.HasEquipment(EquipmentType.Dash)) {
                if (pendingEquipmentType == EquipmentType.Dash) {
                    dashButton.interactable = false;
                    dashImage.color = Color.white;
                    dashEquipmentButtonBackground.color = Color.yellow;
                    dashEquipmentButtonBackground.enabled = true;
                } else {
                    dashButton.interactable = true;
                    dashImage.color = Color.white;
                    dashEquipmentButtonBackground.enabled = false;
                }
            } else {
                dashButton.interactable = false;
                dashImage.color = Color.black;
                dashEquipmentButtonBackground.enabled = false;
            }
            Button healthPackButton = healthPackEquipmentButtonObject.GetComponent<Button>();
            Image healthPackImage = healthPackButton.GetComponent<Image>();
            healthPackImage.sprite = equipmentBlueprints[EquipmentType.HealthPack].IconLarge;
            healthPackEquipmentButtonBackground.sprite = equipmentBlueprints[EquipmentType.HealthPack].IconLarge;
            Debug.Log("player has health pack: " + player.HasEquipment(EquipmentType.HealthPack));
            if (player.HasEquipment(EquipmentType.HealthPack)) {
                if (pendingEquipmentType == EquipmentType.HealthPack) {
                    healthPackButton.interactable = false;
                    healthPackImage.color = Color.white;
                    healthPackEquipmentButtonBackground.color = Color.yellow;
                    healthPackEquipmentButtonBackground.enabled = true;
                } else {
                    healthPackButton.interactable = true;
                    healthPackImage.color = Color.white;
                    healthPackEquipmentButtonBackground.enabled = false;
                }
            } else {
                healthPackButton.interactable = false;
                healthPackImage.color = Color.black;
                healthPackEquipmentButtonBackground.enabled = false;
            }
            Button shieldButton = shieldEquipmentButtonObject.GetComponent<Button>();
            Image shieldImage = shieldButton.GetComponent<Image>();
            shieldImage.sprite = equipmentBlueprints[EquipmentType.Shield].IconLarge;
            shieldEquipmentButtonBackground.sprite = equipmentBlueprints[EquipmentType.Shield].IconLarge;
            Debug.Log("player has shield: " + player.HasEquipment(EquipmentType.Shield));
            if (player.HasEquipment(EquipmentType.Shield)) {
                if (pendingEquipmentType == EquipmentType.Shield) {
                    shieldButton.interactable = false;
                    shieldImage.color = Color.white;
                    shieldEquipmentButtonBackground.color = Color.yellow;
                    shieldEquipmentButtonBackground.enabled = true;
                } else {
                    shieldButton.interactable = true;
                    shieldImage.color = Color.white;
                    shieldEquipmentButtonBackground.enabled = false;
                }
            } else {
                shieldButton.interactable = false;
                shieldImage.color = Color.black;
                shieldEquipmentButtonBackground.enabled = false;
            }
        }

        void OpenEquipmentPanel() {
                SetEquipmentPanelDetails(GameModel.Instance.player.currentEquipmentType);
                pendingEquipmentType = GameModel.Instance.player.currentEquipmentType;
        

            
            SetupEquipmentButtons();
        }

        private struct PendingTotemCthulk {
            public string hairStyle;
            public Color32 primaryColour;
            public Color32 secondaryColour;
            public PendingTotemCthulk(string hairStyle, Color32 primaryColour, Color32 secondaryColour) {
                this.hairStyle = hairStyle;
                this.primaryColour = primaryColour;
                this.secondaryColour = secondaryColour;
            }

        }

        private struct PendingTotemHarpoon {
            public string material;
            public string element;
            public Color32 primaryColour;
            public Color32 secondaryColour;

            public PendingTotemHarpoon(string material, string element, Color32 primaryColour, Color32 secondaryColour) {
                this.material = material.ToLower();
                this.element = element.ToLower();
                this.primaryColour = primaryColour;
                this.secondaryColour = secondaryColour;
            }

        }
        private bool hasPendingTotemCthulk = false;
        private bool hasPendingTotemHarpoon = false;
        private PendingTotemCthulk pendingTotemCthulk;
        private PendingTotemHarpoon pendingTotemHarpoon;

        

        public void OpenCraftingUI() {
            currentPanelIndex = 1;
            ChangePanel(panels[currentPanelIndex]);
            //refresh which blueprints are unlocked.
            foreach (ICraftBlueprint blueprint in blueprints.Values) {
                blueprint.isUnlocked = GameModel.Instance.saveGame.rewardsUnlocked[blueprint.RewardType];
            }
            //refresh which equipment is owned.
            numEquipmentOwned = 0;
            foreach (EquipmentType type in equipmentTypes) {
                if (GameModel.Instance.player.HasEquipment(type)) {
                    numEquipmentOwned++;
                }
            }
        }

        void ApplyTotem() {
            if (hasPendingTotemCthulk) {
                Debug.Log("Applying Totem Cthulk");
                GetComponent<TotemApplier>().ApplyTotemCthulk(pendingTotemCthulk.hairStyle, pendingTotemCthulk.primaryColour, pendingTotemCthulk.secondaryColour);
            }
            if (hasPendingTotemHarpoon) {
                GetComponent<TotemApplier>().ApplyTotemHarpoon(pendingTotemHarpoon.material, pendingTotemHarpoon.element, pendingTotemHarpoon.primaryColour, pendingTotemHarpoon.secondaryColour);
            }
        }

        public void CloseCraftingUI() {
            if (pendingEquipmentType != GameModel.Instance.player.currentEquipmentType) {
                GameModel.Instance.player.ChangeEquipment(pendingEquipmentType);
            }
            ApplyTotem();
        }

        public void TabButtonLeft() {
            currentPanelIndex--;
            if (currentPanelIndex < 0) currentPanelIndex = panels.Length - 1;
            ChangePanel(panels[currentPanelIndex]);
        }

        public void TabButtonRight() {
            currentPanelIndex++;
            if (currentPanelIndex >= panels.Length) currentPanelIndex = 0;
            ChangePanel(panels[currentPanelIndex]);
        }

        void Update() {
            if (CthulkInput.ActivateKeyDown() || CthulkInput.EscapeKeyDown()) {
                UIManager.Instance.CloseCraftingMenu();
            }
        }
    }
}