using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class ShieldEquipment : MonoBehaviour, IPlayerEquipment {
        [SerializeField] private SpriteRenderer offSprite;
        [SerializeField] private SpriteRenderer activeSprite;
        [SerializeField] private GameObject shieldObject;
        [SerializeField] private float shieldMaxDuration = 3f;
        [SerializeField] private float shieldRecoveryTime = 6f;
        [SerializeField] private float shieldBreakCooldown = 10f;
        [SerializeField] private float shieldMinStrengthToUse = 0.3f;
        [SerializeField] private float shieldTogglePenalty = 0.1f;

        public EquipmentType equipmentType {get;} = EquipmentType.Shield;
        public PlayerStateName usageState {get;} = PlayerStateName.staticEquipment;
        public bool isActive {get; private set;} = false;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Hold;
        private float shieldStrength;
        private float shieldBrokenTime = -99f;

        public bool ActivationCondition(Player player) {
            return Time.time > shieldBrokenTime + shieldBreakCooldown && shieldStrength > shieldMinStrengthToUse;
        }
        public void Activate(Player player) {
            Debug.Log("Shield Activated");
            isActive = true;
            shieldObject.SetActive(true);
        }
        public void CancelActivation(Player player) {
            Debug.Log("Shield Cancelled");
            isActive = false;
            shieldObject.SetActive(false);
            shieldStrength = Mathf.Max(0, shieldStrength - (shieldTogglePenalty*shieldMaxDuration));
        }
        public void Equip(Player player) {
            offSprite.enabled = true;
            activeSprite.enabled = false;
            shieldStrength = shieldMaxDuration;
        }
        public void Unequip(Player player) {
            offSprite.enabled = false;
            activeSprite.enabled = false;
        }

        public void TakeDamage(Player player) {
            if (isActive) {
                player.DeactivateEquipment();
                shieldBrokenTime = Time.time;
            }
        }

        public void UpdateEquipment(Player player) {
            if (!isActive) {
                float sheildRecoveryPerSecond = shieldMaxDuration / shieldRecoveryTime;
                shieldStrength = Mathf.Min(shieldMaxDuration, shieldStrength + (Time.deltaTime * sheildRecoveryPerSecond));
                
                if (ActivationCondition(player)) {
                    offSprite.enabled = false;
                    activeSprite.enabled = true;
                } else {
                    offSprite.enabled = true;
                    activeSprite.enabled = false;
                }
            } else {
                shieldStrength = Mathf.Max(0, shieldStrength - Time.deltaTime);
                if (shieldStrength <= 0) {
                    player.DeactivateEquipment();
                }
            }
        }
    }
}