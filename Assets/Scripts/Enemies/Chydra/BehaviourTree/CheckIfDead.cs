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

        


        public override TaskStatus OnUpdate()
        {
            survivingheads = 0;
            for (int i = 0; i < 3; i++)
            {
                if (healths[i].Value > 0 && headsActive.Value - 1 >= i)
                {
                    survivingheads.Value++;
                }
                if (healths[i].Value <= 0 && heads[i].Value.activeInHierarchy)
                {
                    heads[i].Value.GetComponent<Animator>().SetBool("Dead",true);
                    heads[i].Value.GetComponent<Animator>().SetTrigger("Die");
                    survivingheads.Value--;
                }
              

                if (survivingheads.Value <= 0)
                {

                    switch (headsActive)
                    {
                        case var value when value.Value == 1:
                            headsActive = 2;


                            healths[0].Value = 50;
                            heads[0].Value.SetActive(true);
                            heads[1].Value.SetActive(true);
                            survivingheads.Value = 2;
                            heads[0].Value.GetComponent<Animator>().SetBool("Dead", false);
                            break;
                        case var value when value.Value == 2:
                            headsActive = 3;
                            healths[0].Value = 50;
                            healths[1].Value = 50;

                            heads[0].Value.GetComponent<Animator>().SetBool("Dead", false);
                            heads[1].Value.GetComponent<Animator>().SetBool("Dead", false);
                            heads[0].Value.SetActive(true);
                            heads[1].Value.SetActive(true);
                            heads[2].Value.SetActive(true);
                            survivingheads.Value = 3;
                            break;
                        case var value when value.Value == 3:
                            headsActive = 0;
                            for (int x = 0; x < 3; x++)
                            {
                                heads[0].Value.GetComponent<Animator>().SetBool("Dead", false);
                                heads[1].Value.GetComponent<Animator>().SetBool("Dead", false);
                               // heads[x].Value.SetActive(false);
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
            }
                
                return TaskStatus.Failure;
        }

       

    }

}