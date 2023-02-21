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
        TotemPanel 
    }
    public class CraftingUI : MonoBehaviour
    {
        [SerializeField] private GameObject equipmentPanel;
        [SerializeField] private GameObject storePanel;
        [SerializeField] private GameObject totemPanel;

        [Header("Store Panel")]
        [SerializeField] private GameObject MakeASelectionText;
        [SerializeField] private GameObject TemplateStoreItem;
        [SerializeField] private GameObject StoreContentBox;
        [SerializeField] private GameObject StoreDetailsPanel;
        [SerializeField] private TextMeshProUGUI StoreDetailsTitle;
        [SerializeField] private TextMeshProUGUI StoreDetailsSubtitle;
        [SerializeField] private TextMeshProUGUI StoreDetailsDescription;
        [SerializeField] private TextMeshProUGUI StoreDetailsCost;
        [SerializeField] private Image StoreDetailsImage;
        [SerializeField] private Button StoreCraftButton;

        [Header("Equipment Panel")]
        [SerializeField] private TextMeshProUGUI equipmentTitleText;
        [SerializeField] private TextMeshProUGUI equipmentDescriptionText;
        [SerializeField] private string NoEquipmentText = "No Equipment";
        [SerializeField] private GameObject dashEquipmentButtonObject;
        [SerializeField] private Image dashEquipmentButtonBackground;
        [SerializeField] private GameObject healthPackEquipmentButtonObject;
        [SerializeField] private Image healthPackEquipmentButtonBackground;
        [SerializeField] private GameObject shieldEquipmentButtonObject;
        [SerializeField] private Image shieldEquipmentButtonBackground;

        public CraftUIState CurrentState { get; private set; }

        private int currentPanelIndex = 1;
        private CraftUIState[] panels = new CraftUIState[] { CraftUIState.EquipmentPanel, CraftUIState.StorePanel, CraftUIState.TotemPanel };

        private Dictionary<RewardType, ICraftBlueprint> blueprints = new Dictionary<RewardType, ICraftBlueprint>();
        private Dictionary<EquipmentType, ICraftBlueprint> equipmentBlueprints = new Dictionary<EquipmentType, ICraftBlueprint>();
        private ICraftBlueprint selectedBlueprint;

        private EquipmentType[] equipmentTypes = new EquipmentType[] { EquipmentType.Dash, EquipmentType.HealthPack, EquipmentType.Shield };
        private int numEquipmentOwned = 0;

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

        void ChangePanel(CraftUIState state) {
            CurrentState = state;
            switch (state) {
                case CraftUIState.EquipmentPanel:
                    equipmentPanel.SetActive(true);
                    storePanel.SetActive(false);
                    totemPanel.SetActive(false);
                    OpenEquipmentPanel();
                    break;
                case CraftUIState.StorePanel:
                    equipmentPanel.SetActive(false);
                    storePanel.SetActive(true);
                    totemPanel.SetActive(false);
                    ClearStorePanelDetails();
                    CreateCraftingOptions();
                    break;
                case CraftUIState.TotemPanel:
                    equipmentPanel.SetActive(false);
                    storePanel.SetActive(false);
                    totemPanel.SetActive(true);
                    break;
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
                StoreDetailsTitle.text = selectedBlueprint.Title;
                StoreDetailsSubtitle.text = selectedBlueprint.Subtitle;
                StoreDetailsDescription.text = selectedBlueprint.Description;
                StoreDetailsCost.text = selectedBlueprint.Cost.ToString();
                StoreDetailsImage.sprite = selectedBlueprint.IconLarge;

                StoreCraftButton.interactable = !selectedBlueprint.AlreadyOwns(GameModel.Instance.player) && GameModel.Instance.player.PlayerHasMoney(selectedBlueprint.Cost);
                StoreDetailsPanel.SetActive(true);
            }
        }

        void ClearStorePanelDetails() {
            selectedBlueprint = null;
            MakeASelectionText.SetActive(true);
            StoreDetailsTitle.text = "";
            StoreDetailsSubtitle.text = "";
            StoreDetailsDescription.text = "";
            StoreDetailsCost.text = "";
            StoreDetailsImage.sprite = null;
            StoreCraftButton.interactable = false;
            StoreDetailsPanel.SetActive(false);
        }

        public void StoreCraftingButtonPressed() {
            if (selectedBlueprint != null) {
                selectedBlueprint.Craft(GameModel.Instance.player);
                switch (selectedBlueprint.BlueprintType) {
                    case BlueprintType.Equipment:
                        if (numEquipmentOwned == 0) {
                            foreach (EquipmentType type in equipmentTypes) {
                                if (GameModel.Instance.player.HasEquipment(type)) {
                                    GameModel.Instance.player.ChangeEquipment(type);
                                    break;
                                }
                            }
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
            foreach (ICraftBlueprint blueprint in blueprints.Values) {
                if (!blueprint.AlreadyOwns(GameModel.Instance.player) && GameModel.Instance.saveGame.rewardsUnlocked[blueprint.RewardType]) {
                    GameObject storeItem = Instantiate(TemplateStoreItem, StoreContentBox.transform);
                    RectTransform rect = storeItem.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, rect.anchoredPosition.y - (numButtons * 120));
                    numButtons++;
                    storeItem.GetComponent<Button>().onClick.AddListener(() => SetStorePanelDetails(blueprint.RewardType));
                    storeItem.transform.Find("Image").gameObject.GetComponent<Image>().sprite = blueprint.IconSmall;
                    storeItem.transform.Find("Cost").gameObject.GetComponent<TextMeshProUGUI>().text = blueprint.Cost.ToString();
                    storeItem.SetActive(true);
                }
            }
            TemplateStoreItem.SetActive(false);
        }

        //equipment panel

        
        void SetEquipmentPanelDetails(EquipmentType type) {
            equipmentTitleText.text = equipmentBlueprints[type].Title;
            equipmentDescriptionText.text = equipmentBlueprints[type].Description;
        }
        public void SelectDash() {
            SetEquipmentPanelDetails(EquipmentType.Dash);
            GameModel.Instance.player.ChangeEquipment(EquipmentType.Dash);
            SetupEquipmentButtons();
        }

        public void SelectHealthPack() {
            SetEquipmentPanelDetails(EquipmentType.HealthPack);
            GameModel.Instance.player.ChangeEquipment(EquipmentType.HealthPack);
            SetupEquipmentButtons();
        }

        public void SelectShield() {
            SetEquipmentPanelDetails(EquipmentType.Shield);
            GameModel.Instance.player.ChangeEquipment(EquipmentType.Shield);
            SetupEquipmentButtons();
        }

        void SetupEquipmentButtons() {
            Dictionary<EquipmentType, bool> ownedEquipment = GameModel.Instance.saveGame.equipmentBuilt;
            Button dashButton = dashEquipmentButtonObject.GetComponent<Button>();
            Image dashImage = dashButton.GetComponent<Image>();
            dashImage.sprite = equipmentBlueprints[EquipmentType.Dash].IconLarge;
            dashEquipmentButtonBackground.sprite = equipmentBlueprints[EquipmentType.Dash].IconLarge;
            if (ownedEquipment[EquipmentType.Dash]) {
                if (GameModel.Instance.player.currentEquipmentType == EquipmentType.Dash) {
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
            if (ownedEquipment[EquipmentType.HealthPack]) {
                if (GameModel.Instance.player.currentEquipmentType == EquipmentType.HealthPack) {
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
            if (ownedEquipment[EquipmentType.Shield]) {
                if (GameModel.Instance.player.currentEquipmentType == EquipmentType.Shield) {
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
            if (numEquipmentOwned > 0) {
                SetEquipmentPanelDetails(GameModel.Instance.player.currentEquipmentType);
            } else {
                equipmentDescriptionText.text = NoEquipmentText;
                equipmentTitleText.text = "";
            }
            SetupEquipmentButtons();
        }




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
            if (CthulkInput.ActivateKeyDown()) {
                UIManager.Instance.CloseCraftingMenu();
            }
        }
    }
}