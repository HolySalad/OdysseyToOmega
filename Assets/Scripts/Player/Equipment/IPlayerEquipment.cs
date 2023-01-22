using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.Equipment {
    public enum EquipmentType {
        None,
        HarpoonLauncher,
        Shield,
        Dash,
        HealthPack
    }
    public enum EquipmentActivationBehaviour {
        None,
        Hold,
        Press,
        Toggle

    }

    public interface IPlayerEquipment {

        public EquipmentType equipmentType {get;}
        public bool isActive {get;}
        public EquipmentActivationBehaviour activationBehaviour {get;}
        public bool ActivationCondition(Player player);
        public void Activate(Player player);
        public void CancelActivation(Player player);
        public void Equip(Player player);
        public void Unequip(Player player);
    }

}