using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers
{
    public class ChydraAttachPlayer : StateMachineBehaviour
    {
        private GameObject target;
        [SerializeField] private float fireballSpeed = 15f;
        private Vector2 velocity;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            GameObject gameobject = animator.gameObject;
            HydraMain generalObject = gameobject.GetComponent<HydraMain>();


            // lastSailIndex = targetSailIndex;
            float xPos = generalObject.mouthposition.transform.position.x;
            float yPos = generalObject.mouthposition.transform.position.y;


            GameObject fireballObject = Instantiate(generalObject.fireballPrefab, new Vector2(xPos, yPos), Quaternion.identity);
            Fireball fireball = fireballObject.GetComponent<Fireball>();
            fireball.SetupMeteor(fireballSpeed, fireballObject.transform.position, generalObject.player, generalObject.meteorSoundDuration);
        }

    }
}