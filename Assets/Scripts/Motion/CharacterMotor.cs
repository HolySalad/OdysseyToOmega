using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Movement {

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

        public void ClearModifiers(string type) {
            List<IMovementModifier> newMoveMods = new List<IMovementModifier>();
            foreach (IMovementModifier modifier in movementModifiers) {
                if (modifier.GetType().ToString() != type) {
                    newMoveMods.Add(modifier);
                    Debug.Log("Clear modifiers removed " + type);
                }
            }
            movementModifiers = newMoveMods;
        }

        // on FixedUpdate, move the character
        void FixedUpdate()
        {
            Move();
        }

        public void AddMovementModifier(IMovementModifier modifier)
        {
            Debug.Log("AddMovementModifier " + modifier.GetType().ToString());
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
                //if (modifier.Enabled) {
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
                //}
            }
            rb.velocity = movement;

        }
    }
}