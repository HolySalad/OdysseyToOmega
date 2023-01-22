using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class HealthPackEquipment : MonoBehaviour, IPlayerEquipment {
        [SerializeField] private SpriteRenderer sprite;
        public EquipmentType equipmentType {get;} = EquipmentType.HealthPack;
        public bool isActive {get;} = false;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Toggle;
        public bool ActivationCondition(Player player) {
            return false;
        }
        public void Activate(Player player) {
            
        }
        public void CancelActivation(Player player) {
            
        }
        public void Equip(Player player) {
            sprite.enabled = true;
        }
        public void Unequip(Player player) {
            sprite.enabled = false;
        }
    }
}