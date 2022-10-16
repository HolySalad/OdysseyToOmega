using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Items {
    public interface IHeldItems
    {
        public bool canBeUsed { get; }
        public bool isHeld {get; }

        public Sprite itemSprite {get;}

        public string itemName {get;}
        public string helpText {get;}
        public void Input();

        public void HeldMode();

        public void DropMode();

    }
}
