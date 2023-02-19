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
    }
}