using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Movement;

namespace SpaceBoat.Player {
    public class PlayerLogic : MonoBehaviour
    {
        //serialized
        [SerializeField] private float maxHealth = 3;

        //movement behaviours
        private NormalWalk defaultWalk;
        private NormalJump defaultjump;

        //controller
        private PlayerInput input;


        //On Awake, initialize movement behaviours and enable them
        //initialize the player controller with the movement behaviours
        void Awake() {
            Debug.Log("Enabling Movement Behaviours");
            defaultWalk = GetComponent<NormalWalk>();
            defaultWalk.OnEnable();
            defaultjump = GetComponent<NormalJump>();
            defaultjump.OnEnable();
            
            Debug.Log("Initializing Player Controller");
            input = GetComponent<PlayerInput>();
            input.Init(defaultWalk, defaultjump);

            Debug.Log("Player Logic Awoke");
        }

        void PlayerEntersShip() {

        }

        void PlayerExitsShip() {

        }

        void OnColliderEnter2D(Collider2D other) {
            if (other.gameObject.tag == "Ship") {
                PlayerEntersShip();
            }
        }
    }
}
