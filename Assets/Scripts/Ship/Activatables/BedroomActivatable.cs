using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Ship.Activatables {
    public class BedroomActivatable : MonoBehaviour, IActivatables
    {
        [SerializeField] private UI.HelpPrompt helpPrompt;
        public UI.HelpPrompt activatableHelpPrompt {get {return helpPrompt;}}

        [SerializeField] private UI.HelpPrompt inUseHelpPrompt;
        public UI.HelpPrompt activatableInUseHelpPrompt {get {return inUseHelpPrompt;}}
        public bool supressPromptDuringTutorial {get;} = true;
        public ActivatablesNames kind {get;} = ActivatablesNames.Bedroom;
        public bool isInUse {get; private set;} = false;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerStateName playerState {get;} = PlayerStateName.ready;
        public string usageAnimation {get;} = "";
        public string usageSound {get;} = "";

        public void Activate(Player player) {
            Debug.Log("Player activated bedroom equipment station");
            isInUse = true;
            foreach (UsageCallback callback in usageCallbacks) {
                callback();
            }
            GameModel.Instance.saveGameManager.Save();
        }

        public void Deactivate(Player player) {
            isInUse = false;            
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
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