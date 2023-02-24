using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BehaviorDesigner.Runtime.Tasks.Unity.UnityAnimator
{
    public class TakeDamage : Conditional
    {
        public string tagg;
        public TriggerEnter2D triggered;
        public SharedGameObject headObject;
        public SharedInt health;
        public string animationTriggerName;

        public int healthLoss;


        public override void OnAwake()
        {
            triggered.tagg = tagg;
        }
        public override TaskStatus OnUpdate()
        {
            if (triggered.triggered)
            {
           
                if(health.Value>0)
                {
                health.Value -= healthLoss;

                headObject.Value.GetComponent<Animator>().SetTrigger(animationTriggerName);
                headObject.Value.GetComponent<Animator>().SetBool("BreakHookBool",true);
                }
                else
                {
                    headObject.Value.GetComponent<Animator>().SetBool("Dead", true);
                    headObject.Value.GetComponent<Animator>().SetTrigger("Die");
                }
           

                triggered.triggered = false;
                return TaskStatus.Success;
                
            }

          
                return TaskStatus.Failure;
            
        }

  

    }
}