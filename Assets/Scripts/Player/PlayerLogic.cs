using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Movement;

namespace SpaceBoat.Player {
    public class PlayerLogic : MonoBehaviour
    {
        //serialized
        [SerializeField] private float maxHealth = 3;
        [SerializeField] private int hitInvulnerabilityFrames = 50;

        //movement behaviours
        private CharacterMotor motor;
        private NormalWalk defaultWalk;
        private NormalJump defaultjump;
        //private InsideShipJump insideShipJump;
        private Animator animator;

        //controller
        private PlayerInput input;

        //game state
        private float health;
        private bool playerDiedFailure = false;
        private bool shipDiedFailure = false;

        //contextual
        private bool isBelowDeck = false;

        private int numSails = 0;
        private int numBrokenSails = 0;
        
        

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

            //animator
            animator = GetComponent<Animator>();
            
            Debug.Log("Initializing Player Controller");
            input = GetComponent<PlayerInput>();
            input.Init(defaultWalk, defaultjump);

            Debug.Log("Player Logic Awoke");

            health = maxHealth;
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
            if (defaultWalk.hitOnFrame + hitInvulnerabilityFrames > Time.frameCount) {
                return;
            }
            Debug.Log("Player takes "+ damage + " damage");
            health -= damage;
            defaultWalk.hitOnFrame = Time.frameCount;
            defaultjump.hitOnFrame = Time.frameCount;
            if (health <= 0) {
                PlayerDies();
            } else {
                animator.SetTrigger("Hit");
                //TODO sound
            }
        }

        public void PlayerHeals() {
            Debug.Log("Player heals");
            health = maxHealth;
        }

        void PlayerDies() {
            Debug.Log("Player Died");
            playerDiedFailure = true;
            Time.timeScale = 0;    
            animator.SetTrigger("Dead");
            //TODO sound
            //TODO scene transition to game over.
        }

        void PlayerDies(bool scream) {
            if (scream) {
                //TODO sound queue for scream
            } else {
                PlayerDies();
            }
        }


        //sails

        void ShipDies() {
            Debug.Log("Ship Died");
            shipDiedFailure = true;
            Time.timeScale = 0;
            //TODO sound
        }

        public void RegisterSail() {
            numSails++;
        }

        public void SailBreaks() {
            numBrokenSails++;
            if (numBrokenSails >= numSails) {
                ShipDies();
            }
        }

        public void SailRepairs() {
            numBrokenSails--;
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
            GUI.Label(new Rect(10, 30, 100, 20), "Sails: " + (numSails-numBrokenSails) + "/" + numSails);
            if (playerDiedFailure) {
                GUI.Label(new Rect(Screen.width/2, Screen.height/2, Screen.width/2, Screen.height/2), "You Died");
            }
            if (shipDiedFailure) {
                GUI.Label(new Rect(Screen.width/2, Screen.height/2, 100, 20), "The Ship Sank");
            }
        }

    }
}
