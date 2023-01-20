using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Environment {    
    public interface IBouncable 
    {
        public void Bounce(Player player);
    }
}