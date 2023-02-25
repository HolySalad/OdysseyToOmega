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
           
                    if (headObject.Value.GetComponent<Animator>().GetBool("H2Awaken") == false && headObject.Value.GetComponent<Animator>().GetBool("H3Awaken") == false)
                    {
                health.Value -= healthLoss;
                    }
                if (headObject.Value.GetComponent<Animator>().GetBool("Dead") == false)
                {


                    if (health.Value > 0)
                    {

                        headObject.Value.GetComponent<Animator>().SetTrigger(animationTriggerName);
                        headObject.Value.GetComponent<Animator>().SetBool("BreakHookBool", true);

                    }
                    else
                    {
                        headObject.Value.GetComponent<Animator>().SetBool("Dead", true);
                        headObject.Value.GetComponent<Animator>().SetTrigger("Die");
                    }
                }
           

                triggered.triggered = false;
                return TaskStatus.Success;
                
            }

          
                return TaskStatus.Failure;
            
        }

  

    }
}