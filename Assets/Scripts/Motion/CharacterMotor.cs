using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LittleDeath.Movement {

public class CharacterMotor : MonoBehaviour
    {


        private Rigidbody2D rb;
        private List<IMovementModifier> movementModifiers = new List<IMovementModifier>();

        // on Awake, add references to rigidbody, CharacterMotor and collider
        void Awake()
        {
            rb = this.gameObject.GetComponent<Rigidbody2D>();
        }

        public void ClearModifiers() {
            movementModifiers = new List<IMovementModifier>();
        }

        // on FixedUpdate, move the character
        void FixedUpdate()
        {
            Move();
        }

        public void AddMovementModifier(IMovementModifier modifier)
        {
            movementModifiers.Add(modifier);
        }

        public void RemoveMovementModifier(IMovementModifier modifier)
        {
            movementModifiers.Remove(modifier);
        }

        // apply movement modifiers to the character
        private void Move()
        {
            Vector2 movement = Vector2.zero;
            foreach (IMovementModifier modifier in movementModifiers)
            {
                movement += modifier.Value;
                //print("Character Motor " + modifier.GetType().Name + " Value: " + modifier.Value);
                modifier.UpdateModifier(Time.fixedDeltaTime);
                    if (modifier is IJump) {
                        IJump jump = (IJump)modifier;
                        jump.UpdateAnimator();
                    }
                    if (modifier is IWalk) {
                        IWalk walk = (IWalk)modifier;
                        walk.UpdateAnimator();
                    }
            }
            rb.velocity = movement;

        }
    }
}