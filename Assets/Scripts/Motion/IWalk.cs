using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LittleDeath.Movement {
    public interface IWalk
    {
        public float lastHorizontal {get;}
        // holds the current speed
        public float speed {get;}
    
        public bool FacingRight {get;}
        public bool IsWalking {get;}
        void Input(float horizontal);

        void UpdateAnimator();
    }
}
