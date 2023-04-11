using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class DashEquipment : MonoBehaviour, IPlayerEquipment
    {
        [SerializeField] private Light2D activeLight;

        [SerializeField] private float dashCooldown = 2f;
        [SerializeField] private float dashDuration = 0.5f;
        [SerializeField] public float dashSpeed = 20f;
        [SerializeField] public float maintainedMomentumMultiplier = 0.5f;

        public EquipmentType equipmentType {get;} = EquipmentType.Dash;
        public PlayerStateName usageState {get;} = PlayerStateName.dash;
        public bool isActive {get; private set;} = false;
        public bool canCancelWorkToUse {get;} = false;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Press;
        private float cooldown = 0f;
        private float dashTimer = 0f;
        public bool hasLandedSinceLastDash = true;

        private Player player;
        private EquipmentSpriteManager spriteManager;

        void Awake() {
            player = GetComponent<Player>();
            spriteManager = GetComponent<EquipmentSpriteManager>();
        }

        public bool ActivationCondition(Player player) {
            return (cooldown <= 0f && hasLandedSinceLastDash);
        }
        public void Activate(Player player) {
            isActive = true;
            cooldown = dashCooldown;
            dashTimer = dashDuration;
            spriteManager.SetDisplayedSprite("DashActive");
            if (activeLight != null) activeLight.enabled = true;
            SoundManager.Instance.Play("Dash");
        }
        public void CancelActivation(Player player) {
            isActive = false;
            spriteManager.SetDisplayedSprite("DashEmpty");
            if (activeLight != null) activeLight.enabled = false;
        }
        public void Equip(Player player) {
        }
        public void Unequip(Player player) {
            activeLight.enabled = false;
        }


        public void UpdateEquipment(Player player) {
            if (cooldown > 0f) {
                cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
                if (cooldown > dashCooldown * 2f / 3f) {
                    spriteManager.SetDisplayedSprite("DashEmpty");
                } else if (cooldown > dashCooldown * 1f / 3f) {
                    spriteManager.SetDisplayedSprite("DashOneThird");
                } else {
                    spriteManager.SetDisplayedSprite("DashTwoThirds");
                }
            } else 
                spriteManager.SetDisplayedSprite("DashReady");
                
            if (isActive) {
                dashTimer = Mathf.Max(0f, dashTimer - Time.deltaTime);
                if (dashTimer <= 0f) {
                    player.DeactivateEquipment();
                }
            }
        }
    }

}