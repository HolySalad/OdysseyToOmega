using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship.Activatables {
    public class KitchenActivatable : MonoBehaviour, IActivatables
    {   
        [SerializeField] private UI.HelpPrompt helpPrompt;
        public UI.HelpPrompt activatableHelpPrompt {get {return helpPrompt;}}

        [SerializeField] private UI.HelpPrompt inUseHelpPrompt;
        public UI.HelpPrompt activatableInUseHelpPrompt {get {return inUseHelpPrompt;}}    
        public bool supressPromptDuringTutorial {get;} = false;
        [SerializeField] private float healTime = 3;

        public ActivatablesNames kind {get;} = ActivatablesNames.Kitchen;
        public bool isInUse {get; private set;} = false;
        private SpriteRenderer spriteRenderer;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerStateName playerState {get;} = PlayerStateName.working;
        public string usageAnimation {get;} = "Cooking";
        public string usageSound {get;} = "Cooking";


        private float timeBeganCooking = 0;

        private Player player;


        private List<UsageCallback> usageCallbacks = new List<UsageCallback>();
        private List<UsageCallback> deactivationCallbacks = new List<UsageCallback>();
        public void AddActivationCallback(UsageCallback callback) {
            usageCallbacks.Add(callback);
        }
        public void AddDeactivationCallback(UsageCallback callback) {
            deactivationCallbacks.Add(callback);
        }

        public void Activate(Player player) {
            this.player = player;
            isInUse = true;
            timeBeganCooking = Time.time;
            foreach (UsageCallback callback in usageCallbacks) {
                callback();
            }
        }

        public void Deactivate(Player player) {
            isInUse = false;
            foreach (UsageCallback callback in deactivationCallbacks) {
                callback();
            }
        }

        public bool ActivationCondition(Player player) {
            return player.health < player.maxHealth;
        }

        void Update() {
            if (isInUse) {
                if (Time.time - timeBeganCooking >= healTime) {
                    player.PlayerHeals();
                    SoundManager.Instance.Oneshot("ActivationCompleteDing");
                    Deactivate(player);
                    player.DetatchFromActivatable();
                }
            }
        }


    }
}