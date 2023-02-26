using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.HazardManagers
{
    public class ChydraIdleBhvr : StateMachineBehaviour
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

            List<GameObject> sails = GameModel.Instance.shipSails;
            List<GameObject> nonbrokenSails = new List<GameObject>();
            foreach (GameObject sail in sails)
            {
                if (!sail.GetComponent<Ship.Activatables.SailsActivatable>().isBroken)
                {
                    nonbrokenSails.Add(sail);
                }
            }
            int targetSailIndex = Random.Range(0, nonbrokenSails.Count);
            //if (nonbrokenSails.Count > 1 && targetSailIndex == lastSailIndex)
            //{
            //    targetSailIndex = (targetSailIndex + 1) % nonbrokenSails.Count;
            //}
            GameObject randomSail = nonbrokenSails[targetSailIndex];
           // lastSailIndex = targetSailIndex;
            float xPos = generalObject.mouthposition.transform.position.x;
            float yPos = generalObject.mouthposition.transform.position.y;

           
            GameObject fireballObject = Instantiate(generalObject.fireballPrefab, new Vector2(xPos, yPos), Quaternion.identity);
            Fireball fireball = fireballObject.GetComponent<Fireball>();
            fireball.SetupMeteor(fireballSpeed, fireballObject.transform.position, randomSail, generalObject.meteorSoundDuration);
        }

    }
}
