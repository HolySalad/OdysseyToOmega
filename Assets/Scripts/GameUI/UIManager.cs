using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.UI {

    public enum UIState {
        HUD,
        CraftMenu
    }

    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject hudParent;
        [SerializeField] private GameObject craftMenuParent;

        public static UIManager Instance { get; private set; }

        private GameObject buildmodeObject;

        public delegate void OnNextBuildModeExit(bool isCancelled);
        private List<OnNextBuildModeExit> onNextBuildModeExitCallbacks = new List<OnNextBuildModeExit>();
        public void AddOnNextBuildModeExitCallback(OnNextBuildModeExit callback) {
            onNextBuildModeExitCallbacks.Add(callback);
        }

        void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(gameObject);
            }
        }

        void Start() {
            CurrentState = UIState.HUD;
            hudParent.SetActive(true);
            craftMenuParent.SetActive(false);
        }

        public string FixedUIText(string text) {
            return text.Replace("\\n", "\n");
        }

        public UIState CurrentState { get; private set; }

        public void OpenCraftingMenu() {
            CurrentState = UIState.CraftMenu;
            hudParent.SetActive(false);
            craftMenuParent.SetActive(true);
            craftMenuParent.GetComponent<CraftingUI>().OpenCraftingUI();
            GameModel.Instance.PauseGame();
        }

        public void CloseCraftingMenu() {
            CurrentState = UIState.HUD;
            hudParent.SetActive(true);
            craftMenuParent.SetActive(false);
            GameModel.Instance.UnpauseGame();
        }
        private int pendingBuildCost = 0;
        public void EnterBuildMode(GameObject buildablePrefab, int cost) {
            pendingBuildCost = cost;
            buildmodeObject = Instantiate(buildablePrefab, new Vector3(0,0,0), Quaternion.identity);
            craftMenuParent.SetActive(false);
            Transform buildmodeTransform = buildmodeObject.GetComponentInChildren<Ship.Buildables.BuildableExtras.BuildSystemPlacementMarker>().transform;
            GameModel.Instance.cameraController.AddShipViewOverride("UIManager", 100, buildmodeTransform, true, false);
        }

        public void ExitBuildMode(bool isCancelled = false) {
            Destroy(buildmodeObject);
            buildmodeObject = null;
            craftMenuParent.SetActive(true);
            GameModel.Instance.cameraController.RemoveShipViewOverride("UIManager");
            if (!isCancelled) GameModel.Instance.player.PlayerSpendsMoney(pendingBuildCost);
            Debug.Log("UIManager.ExitBuildMode: isCancelled = " + isCancelled);
            Debug.Log("UIManager.ExitBuildMode: onNextBuildModeExitCallbacks.Count = " + onNextBuildModeExitCallbacks.Count);
            foreach (OnNextBuildModeExit callback in onNextBuildModeExitCallbacks) {
                callback(isCancelled);
            }
            onNextBuildModeExitCallbacks.Clear();
            craftMenuParent.GetComponent<CraftingUI>().OpenCraftingUI();
        }
    }
}