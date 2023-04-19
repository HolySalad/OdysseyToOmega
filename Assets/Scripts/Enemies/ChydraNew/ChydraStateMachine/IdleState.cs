using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.HazardManagers;

namespace SpaceBoat.Enemies.ChydraNew {
    public class IdleState : StateMachineBehaviour
    {
        [Header("Head Settings")]
        [SerializeField] private int maxRegularAttacksInARow = 5;
        [SerializeField] private int minRegularAttacksInARow = 2;
        [SerializeField] private int specialAttackChance = 10;
        [SerializeField] private float headIdleTime = 1.5f;

        private ChydraBoss chydraBoss;
        private ChydraController chydraController;

        
        private int regularAttacksInARow = 0;

        private float idleUntilTime = 0f;
        private bool stateChangeTriggered = false;

        // trigger functions 
        void TriggerTargetSails(Animator animator) {
            animator.SetTrigger("AtkSails");
            stateChangeTriggered = true;
        }

        void TriggerFireballAttack(Animator animator) {
            animator.SetTrigger("AtkPlayer");
            stateChangeTriggered = true;
        }



        // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            stateChangeTriggered = false;
            chydraBoss = ChydraBoss.Instance;
            chydraController = animator.GetComponent<ChydraController>();
            chydraController.SetSolverWeightTarget(0.0f);
            idleUntilTime = chydraBoss.HazardTime() + headIdleTime;
        }

        // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
        override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (chydraBoss.HazardTime() < idleUntilTime) {
                return;
            }
            if (stateChangeTriggered) {
                return;
            }
            if (!chydraController.isActivated) {
                return;
            }
            // check ship attacks first.
            if (chydraBoss.ShouldBlockShipInterior()) {
                chydraBoss.BlockedShipInterior();
                //behaviourSystem.SetNextBehaviour(BlockShipInteriorBehaviour);
                return;
            } else if (chydraBoss.ShouldBlockHarpoon()) {
                chydraBoss.BlockedHarpoon();
                //behaviourSystem.SetNextBehaviour(BlockHarpoonBehaviour);
                return;
            } else if (chydraBoss.ShouldTargetSails()) {
                chydraBoss.TargettedSails();
                TriggerTargetSails(animator);
                return;
            }
            if (regularAttacksInARow > maxRegularAttacksInARow || (regularAttacksInARow > minRegularAttacksInARow && Random.Range(0, 100) < specialAttackChance)) {
                // special attack
                regularAttacksInARow = 0;
                int attack = Random.Range(0, 2);
                switch (attack) {
                    case 0:
                        //behaviourSystem.SetNextBehaviour(FastFireballAttackBehaviour);
                        break;
                    case 1:
                        //behaviourSystem.SetNextBehaviour(FireStreamAttackBehaviour);
                        break;
                }
                return;
            } else {
                // regular attack
                TriggerFireballAttack(animator);
                regularAttacksInARow++;
                return;
            }
        }

        // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
        override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (!stateChangeTriggered) {
                //happens when we get knocked out of idle state by being damaged.
            }
        }

        // OnStateMove is called right after Animator.OnAnimatorMove()
        //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that processes and affects root motion
        //}

        // OnStateIK is called right after Animator.OnAnimatorIK()
        //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        //{
        //    // Implement code that sets up animation IK (inverse kinematics)
        //}
    }
}