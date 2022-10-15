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
        private CharacterMotor motor;
        private NormalWalk defaultWalk;
        private NormalJump defaultjump;
        private InsideShipJump insideShipJump;

        //controller
        private PlayerInput input;

        //contextual
        private bool isBelowDeck = false;


        //On Awake, initialize movement behaviours and enable them
        //initialize the player controller with the movement behaviours
        void Awake() {
            motor = GetComponent<CharacterMotor>();
            Debug.Log("Enabling Movement Behaviours");
            defaultWalk = GetComponent<NormalWalk>();
            motor.AddMovementModifier(defaultWalk);
            defaultWalk.OnEnable();
            defaultjump = GetComponent<NormalJump>();
            motor.AddMovementModifier(defaultjump);
            defaultjump.OnEnable();
            //non-default movement behaviours
            //insideShipJump = GetComponent<InsideShipJump>();
            //motor.AddMovementModifier(insideShipJump);
            
            Debug.Log("Initializing Player Controller");
            input = GetComponent<PlayerInput>();
            input.Init(defaultWalk, defaultjump);

            Debug.Log("Player Logic Awoke");
        }

        public void PlayerEntersOrExitsShip(Transform other) {
            float playerYPos = transform.position.y;
            float doorYPos = other.position.y;
            if (playerYPos < doorYPos) {
                isBelowDeck = true;
                //insideShipJump.ReplaceJump(defaultjump);
                //input.jump = insideShipJump;
            } else {
                isBelowDeck = false;
                //defaultjump.ReplaceJump(insideShipJump);
                //input.jump = defaultjump;
            }
            Debug.Log("Player is below deck: " + isBelowDeck);
        }

        public void PlayerTakesDamage(int damage) {
            maxHealth -= damage;
            if (maxHealth <= 0) {
                //player dies
            }
        }

        //events

    }
}
