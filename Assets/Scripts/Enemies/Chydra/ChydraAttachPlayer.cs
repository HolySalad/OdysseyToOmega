using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers
{
    public class ChydraAttachPlayer : StateMachineBehaviour
    {
        
        [Header("Put a number, 1 for the first head awaken, 2 for the second headawaken, 3 for them all to vanish")]
        public int option;

        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
           
            if(option == 1)
            {

            GameModel.Instance.heads[option].SetActive(true);
            animator.SetBool("H2Awaken", false);
            }
            if (option == 2)
            {

                GameModel.Instance.heads[option].SetActive(true);
                animator.SetBool("H3Awaken", false);
            }
            if (option == 3)
            {

                GameModel.Instance.bossParent.SetActive(false);
            }
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {

        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            
      
        }

    }
}