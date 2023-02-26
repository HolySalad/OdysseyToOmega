using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BehaviorDesigner;

namespace SpaceBoat.HazardManagers
{
    public class ChydraAttachPlayer : StateMachineBehaviour
    {
        
        [Header("Put a number, 1 for the first head awaken, 2 for the second headawaken, 3 for them all to vanish")]
        public int option;
        public ChydraInfoKeeper info;
        int used = 1;
        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            info = ChydraInfoKeeper.instance;

                if (option == 1)
                {
                   

                    

                    animator.SetBool("Dead", false);
                    animator.SetBool("H2Awaken", false);

                    used = 1;
                info.CounterOne = 1;
                if(info.CounterOne>0)
                {

                        GameModel.Instance.heads[option].SetActive(true);
                }
                }
            
                if (option == 2 && used != option)
                {


                    
                    animator.SetBool("Dead", false);
                    animator.SetBool("H3Awaken", false);
                used = 2;
                info.CounterTwo++;
                if (info.CounterTwo > 1)
                {

                    GameModel.Instance.heads[option].SetActive(true);
                }
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