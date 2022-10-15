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

        //game state
        private float health;
        private bool playerDiedFailure = false;
        private bool shipDiedFailure = false;

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
            Debug.Log("Player takes "+ damage + " damage");
            maxHealth -= damage;
            if (maxHealth <= 0) {
                PlayerDies();
            }
        }

        void PlayerDies() {
            Debug.Log("Player Died");
            playerDiedFailure = true;
            Time.timeScale = 0;    
        }

        //events
        void OnCollisionEnter2D(Collision2D other) {
            if (other.gameObject.layer == LayerMask.NameToLayer("BottomOfMap") 
            || other.gameObject.layer == LayerMask.NameToLayer("EndOfMapLeft")) {
                Debug.Log("Player died from falling off the ship");
                PlayerDies();
                //Destroy(gameObject); //cant do this if you want to do the death screen from here because it destroys this script.
            }
        }


        //GUI
        //OnGUI, draw the player's health
        //If the player has failed, draw a failure screen
        void OnGUI() {
            GUI.Label(new Rect(10, 10, 100, 20), "Health: " + maxHealth);
            if (playerDiedFailure) {
                GUI.Label(new Rect(Screen.width/2, Screen.height/2, Screen.width/2, Screen.height/2), "You Died");
            }
            if (shipDiedFailure) {
                GUI.Label(new Rect(Screen.width/2, Screen.height/2, 100, 20), "The Ship Sank");
            }
        }

    }
}
