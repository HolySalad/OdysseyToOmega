using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class DashEquipment : MonoBehaviour, IPlayerEquipment
    {
        [SerializeField] private SpriteRenderer offSprite;
        [SerializeField] private SpriteRenderer activeSprite;

        public EquipmentType equipmentType {get;} = EquipmentType.Dash;
        public PlayerStateName usageState {get;} = PlayerStateName.dash;
        public bool isActive {get;} = false;
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Press;

        public bool ActivationCondition(Player player) {
            return false;
        }
        public void Activate(Player player) {
            
        }
        public void CancelActivation(Player player) {
            
        }
        public void Equip(Player player) {
            offSprite.enabled = true;
        }
        public void Unequip(Player player) {
            offSprite.enabled = false;
        }

        public void UpdateEquipment(Player player) {

        }
    }

}