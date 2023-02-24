using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class DashEquipment : MonoBehaviour, IPlayerEquipment
    {
        [SerializeField] private SpriteRenderer offSprite;
        [SerializeField] private SpriteRenderer activeSprite;

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

        public bool ActivationCondition(Player player) {
            return (cooldown <= 0f && hasLandedSinceLastDash);
        }
        public void Activate(Player player) {
            isActive = true;
            cooldown = dashCooldown;
            dashTimer = dashDuration;
            offSprite.enabled = false;
            activeSprite.enabled = true;
        }
        public void CancelActivation(Player player) {
            isActive = false;
            offSprite.enabled = true;
            activeSprite.enabled = false;
        }
        public void Equip(Player player) {
            offSprite.enabled = true;
        }
        public void Unequip(Player player) {
            offSprite.enabled = false;
        }

        public void UpdateEquipment(Player player) {
            if (cooldown > 0f) {
                cooldown = Mathf.Max(0f, cooldown - Time.deltaTime);
            }
            if (isActive) {
                dashTimer = Mathf.Max(0f, dashTimer - Time.deltaTime);
                if (dashTimer <= 0f) {
                    player.DeactivateEquipment();
                }
            }
        }
    }

}