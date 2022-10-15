using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Movement;

namespace SpaceBoat.Movement {
    public class NormalWalk : MonoBehaviour, IMovementModifier, IWalk {
        [Header("Settings")]
        [SerializeField] private float maxSpeed = 8f;
        [SerializeField] private float acceleration = 5f;
        [SerializeField] private float deceleration = 7f;
        [SerializeField] private float accelerationStartMult = 2f;
        [SerializeField] private float accelerationStartRange = 2f;
        [SerializeField] private float turningSpeedMult = 0.7f;

        private CharacterMotor motor;

        // holds the horizontal input
        private float horizontalInput;
        // holds the current speed
        public float lastHorizontal {get; private set;}
        // holds the current speed
        public float speed {get; private set;}
    
        public bool FacingRight {get; private set;} = true;
        public bool IsWalking {get; private set;}

        public Vector2 Value { 
            get
            {
                return new Vector2(speed*lastHorizontal, 0);
            }
        }

        public bool Enabled {get; private set;} = false;
        // on Awake, 
        void Awake()
        {
            motor = this.gameObject.GetComponent<CharacterMotor>();

        }
        // add and remove movement modifiers when enabled or disabled
        public void OnEnable()
        {
            Enabled = true;
        }
        public void OnDisable()
        {
            Enabled = false;
        }

        public void Input(float horizontal) {
            horizontalInput = horizontal;
            FlipSprite();
            if (horizontal != 0) {lastHorizontal = horizontal;}
        }

        public void UpdateModifier(float deltaTime) {
            if (horizontalInput == 0) {
                if (speed > 0) {
                    speed = Mathf.Max(speed - deceleration*deltaTime, 0);
                } else if (speed < 0) {
                    speed = Mathf.Min(speed + deceleration*deltaTime, 0);
                }
            } else {
                float accel = acceleration;
                if (Mathf.Abs(speed) < accelerationStartRange) {
                    accel *= accelerationStartMult;
                }
                speed = Mathf.Min(speed + accel*deltaTime, maxSpeed);
            }
            //TODO send info to animator
        }


        void FlipSprite() {
            if (horizontalInput > 0 && !FacingRight || horizontalInput < 0 && FacingRight) {
                FacingRight = !FacingRight;
                speed = -speed * turningSpeedMult;
            }
        }

        public void UpdateAnimator() {
            //TODO set animator values
        }
    }
}

