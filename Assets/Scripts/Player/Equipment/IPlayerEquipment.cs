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
        public PlayerStateName usageState {get;}
        public EquipmentActivationBehaviour activationBehaviour {get;}
        public bool ActivationCondition(Player player);
        public void Activate(Player player);

        //okay this one is kind of stupid implementation
        // don't call this command from inside of the equipment if you need to have an equipment turn itself off.
        // call it from the player class instead, Player. DeactivateEquipment()
        // that function handles state changes and stuff.
        // it probably would have been smarter to explicitly write them out in every cancel activation function so this wasn't a thing but fuck it.
        public void CancelActivation(Player player);
        public void Equip(Player player);
        public void Unequip(Player player);

        public void UpdateEquipment(Player player);
    }

}