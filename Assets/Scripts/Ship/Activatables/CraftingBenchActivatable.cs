using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship.Activatables {
    public class CraftingBenchActivatable : MonoBehaviour, IActivatables
    {
        [SerializeField] private UI.HelpPrompt helpPrompt;
        public UI.HelpPrompt HelpPrompt {get {return helpPrompt;}}        public ActivatablesNames kind {get;} = ActivatablesNames.CraftingBench;
        public bool isInUse {get; private set;} = false;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerStateName playerState {get;} = PlayerStateName.uiPauseState;
        public string usageAnimation {get;} = "";
        public string usageSound {get;} = "";

        public void Activate(Player player) {
            Debug.Log("Player activated crafting bench");
            isInUse = true;
            foreach (UsageCallback callback in usageCallbacks) {
                callback();
            }
            UI.UIManager.Instance.OpenCraftingMenu();
        }

        public void Deactivate(Player player) {
            isInUse = false;            
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
            UI.UIManager.Instance.CloseCraftingMenu();
        }

        public bool ActivationCondition(Player player) {
            return true;
        }

        private List<UsageCallback> usageCallbacks = new List<UsageCallback>();
        private List<UsageCallback> deactivationCallbacks = new List<UsageCallback>();
        public void AddActivationCallback(UsageCallback callback) {
            usageCallbacks.Add(callback);
        }
        public void AddDeactivationCallback(UsageCallback callback) {
            deactivationCallbacks.Add(callback);
        }
    }
}