using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace BehaviorDesigner.Runtime.Tasks.Unity

{
    public class CheckIfDead : Action

    {

        public SharedGameObject[] heads;
        public SharedInt[] healths;
        public SharedInt headsActive;
        public SharedInt survivingheads;
        public SharedInt maxHealth;

        


        public override TaskStatus OnUpdate()
        {
            survivingheads.Value = 0;
            for (int i = 0; i < headsActive.Value; i++)
            {
                if (healths[i].Value > 0)
                {
                    survivingheads.Value++;
                }
                if (healths[i].Value <= 0)
                {
                    if (!heads[i].Value.GetComponent<Animator>().GetBool("Dead"))
                    {

                       // heads[i].Value.GetComponent<Animator>().SetBool("BreakHookBool", false);
                        heads[i].Value.GetComponent<Animator>().SetBool("Dead", true);
                        heads[i].Value.GetComponent<Animator>().SetTrigger("Die");
                        survivingheads.Value--;
                    }
                }
            }
               


                if (survivingheads.Value <= 0)
                {
               
                    switch (headsActive)
                    {
                        case var value when value.Value == 1:
                            headsActive.Value = 2;
                            healths[0].Value = maxHealth.Value;
                            survivingheads.Value = 2;
                            //heads[0].Value.GetComponent<Animator>().SetBool("Dead", false);
                            heads[0].Value.GetComponent<Animator>().SetBool("H2Awaken", true);
                            break;
                        case var value when value.Value == 2:
                            headsActive.Value = 3;
                            healths[0].Value = maxHealth.Value;
                            healths[1].Value = maxHealth.Value;

                        //heads[0].Value.GetComponent<Animator>().SetBool("Dead", false);
                        //heads[1].Value.GetComponent<Animator>().SetBool("Dead", false);
                        heads[0].Value.GetComponent<Animator>().SetBool("H2Awaken", false);
                        heads[0].Value.GetComponent<Animator>().SetBool("H3Awaken", true);
                            heads[1].Value.GetComponent<Animator>().SetBool("H3Awaken", true);
                            survivingheads.Value = 3;
                            break;
                        case var value when value.Value == 3:
                        heads[0].Value.GetComponent<Animator>().SetBool("H3Awaken", false);
                        heads[1].Value.GetComponent<Animator>().SetBool("H3Awaken", false);
                        for (int x = 0; x < 3; x++)
                            {

                                heads[0].Value.GetComponent<Animator>().SetBool("CompletelyDead", true);
                            }
                            Debug.Log("Finally dead");
                            break;

                        default:
                            Debug.Log("Error, switch case out of bounds.");
                            Debug.Log(headsActive);
                            break;
                    }

                    return TaskStatus.Success;
                }
            
                
                return TaskStatus.Failure;
        }

       

    }

}