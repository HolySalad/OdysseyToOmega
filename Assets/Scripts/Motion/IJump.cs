using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Movement {
    public interface IJump
    {

        public int jumpGrace {get;}
        public bool  jumpSquat {get;}
        public bool isJumping {get;}
        public bool halfJump {get;}
        public float jumpStartTime {get;}
        public float currentVerticalForce {get;}
        public bool isGrounded {get;}
        
        void Input(bool jumpKeyDown);

        void UpdateAnimator();
    }
}
