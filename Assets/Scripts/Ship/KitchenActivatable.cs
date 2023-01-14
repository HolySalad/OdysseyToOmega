using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship {
    public class KitchenActivatable : MonoBehaviour, IActivatables
    {
        [SerializeField] private int healFrames = 72;

        public ActivatablesNames kind {get;} = ActivatablesNames.Kitchen;
        public bool isInUse {get; private set;} = false;
        private SpriteRenderer spriteRenderer;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerStateName playerState {get;} = PlayerStateName.working;
        public string usageAnimation {get;} = "Cooking";
        public string usageSound {get;} = "Cooking";


        private int timeBeganCooking = 0;

        private Player player;

        public void Activate(Player player) {
            this.player = player;
            isInUse = true;
            timeBeganCooking = Time.frameCount;
        }

        public void Deactivate(Player player) {
            isInUse = false;
        }

        public bool ActivationCondition(Player player) {
            return player.health < player.maxHealth;
        }

        void Update() {
            if (isInUse) {
                if (Time.frameCount - timeBeganCooking >= healFrames) {
                    player.PlayerHeals();
                    Deactivate(player);
                    player.DetatchFromActivatable();
                }
            }
        }


    }
}