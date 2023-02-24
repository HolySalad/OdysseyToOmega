using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class HealthPackEquipment : MonoBehaviour, IPlayerEquipment {
        [SerializeField] private SpriteRenderer sprite;
        public EquipmentType equipmentType {get;} = EquipmentType.HealthPack;
        public PlayerStateName usageState {get;} = PlayerStateName.staticEquipment;
        public bool isActive {get; private set;} = false;
        public bool canCancelWorkToUse {get;} = false;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Toggle;

        [SerializeField] private float usageTime = 3f;
        [SerializeField] private float cooldownTime = 15f;


        private float currentUsageTime = 0f;
        private float currentCooldownTime = 0f;

        public bool ActivationCondition(Player player) {
            return currentCooldownTime <= 0f && player.health < player.maxHealth;
        }
        public void Activate(Player player) {
            isActive = true;
            currentUsageTime = usageTime;
        }
        public void CancelActivation(Player player) {
            isActive = false;
        }
        public void Equip(Player player) {
            sprite.enabled = true;
        }
        public void Unequip(Player player) {
            sprite.enabled = false;
        }

        public void UpdateEquipment(Player player) {
            if (!isActive || currentCooldownTime > 0f) {
                currentCooldownTime = Mathf.Max(currentCooldownTime - Time.deltaTime, 0f);
            } else {
                currentUsageTime = Mathf.Max(currentUsageTime - Time.deltaTime, 0f);
                if (currentUsageTime <= 0f) {
                    player.PlayerHeals();
                    currentCooldownTime = cooldownTime;
                    player.DeactivateEquipment();
                }
            }
        }
    }
}