using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship {

    public delegate void UsageCallback();
    public interface IActivatables
    {
        public ActivatablesNames kind {get;}
        public bool isInUse {get;}
        public bool canManuallyDeactivate {get;}
        public PlayerStateName playerState {get;}
        public string usageAnimation {get;}
        public string usageSound {get;}

        public void Activate(Player player);
        public void Deactivate(Player player);
        public bool ActivationCondition(Player player);

        public void AddActivationCallback(UsageCallback callback);
        public void AddDeactivationCallback(UsageCallback callback);
        
    }
}