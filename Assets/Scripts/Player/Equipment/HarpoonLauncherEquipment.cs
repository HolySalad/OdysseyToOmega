using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public class HarpoonLauncherEquipment : MonoBehaviour, IPlayerEquipment
    {
        [SerializeField] private SpriteRenderer offSprite;
        [SerializeField] private SpriteRenderer activeSprite;

        public EquipmentType equipmentType {get;} = EquipmentType.HarpoonLauncher;
        public PlayerStateName usageState {get;} = PlayerStateName.weaponEquipment;
        public bool isActive {get;} = false;
        public bool canCancelWorkToUse {get;} = false;
        
        public EquipmentActivationBehaviour activationBehaviour {get;} = EquipmentActivationBehaviour.Toggle;

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