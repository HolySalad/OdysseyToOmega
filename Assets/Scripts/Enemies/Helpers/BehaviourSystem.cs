using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Enemies.Helpers {
    public class BehaviourSystem : MonoBehaviour
    {
        public enum BehaviourStatus {Continue, ReturnToIdle, GoToNextBehaviour}
        public delegate BehaviourStatus Behaviour(BehaviourSystem behaviourSystem);
        public delegate bool CheckCondition();
        public delegate void BehaviourSystemCallback();

        private Behaviour idleBehaviour;
        private bool hasIdleBehaviour = false;
        private Behaviour currentBehaviour;
        private Behaviour nextBehaviour;
        private bool hasNextBehaviour = false;
        private bool waitingForAnimation = false;
        private AnimatorStateInfo waitingForAnimationState;
        private string waitingForAnimationName;
        private Dictionary<string, BehaviourSystemCallback> behaviourEnterFunctions = new Dictionary<string, BehaviourSystemCallback>();
        private Dictionary<string, BehaviourSystemCallback> behaviourExitFunctions = new Dictionary<string, BehaviourSystemCallback>();
        private Behaviour deathBehaviour;
        private Behaviour damageBehaviour;
        private CheckCondition isDead;
        private bool hasDamageAndDeathBehaviour = false;
        private BehaviourSystemCallback damageFunction;

        public string currentBehaviourName;
        public void SetIdleBehaviour(Behaviour idleBehaviour) {
            this.idleBehaviour = idleBehaviour;
            hasIdleBehaviour = true;
            if (currentBehaviour == null) {
                currentBehaviour = idleBehaviour;
            }
        }
        
        
        //public 
        void SetCurrentBehaviour(Behaviour currentBehaviour) {
            this.currentBehaviour = currentBehaviour;
        }

        public void SetBehaviourEntryFunction(Behaviour behaviour, BehaviourSystemCallback behaviourEnterFunction) {
            behaviourEnterFunctions.Add(behaviour.Method.Name, behaviourEnterFunction);
        }

        public void SetBehaviourExitFunction(Behaviour behaviour, BehaviourSystemCallback behaviourExitFunction) {
            behaviourExitFunctions.Add(behaviour.Method.Name, behaviourExitFunction);
        }

        public void SetNextBehaviour(Behaviour nextBehaviour) {
            this.nextBehaviour = nextBehaviour;
            hasNextBehaviour = true;
        }

        public void SetWaitForAnimation(AnimatorStateInfo stateInfo, string animationName) {
            waitingForAnimationState = stateInfo;
            waitingForAnimationName = animationName;
        }

        public void SetDamageAndDeathBehaviour(Behaviour damageBehaviour, Behaviour deathBehaviour, BehaviourSystemCallback damageFunction, CheckCondition isDead) {
            this.deathBehaviour = deathBehaviour;
            this.damageBehaviour = damageBehaviour;
            this.isDead = isDead;
            this.damageFunction = damageFunction;
            hasDamageAndDeathBehaviour = true;
        }

        public void TakeDamage() {
            if (!hasDamageAndDeathBehaviour) {
                Debug.LogError("No damage and death behaviour set");
            } else {
                damageFunction();
                if (isDead()) {
                    currentBehaviour = deathBehaviour;
                } else {
                    currentBehaviour = damageBehaviour;
                }
            }
        }

        void ChangeBehaviour() {
            if (behaviourExitFunctions.ContainsKey(currentBehaviour.Method.Name)) {
                Debug.Log("Calling exit function for " + currentBehaviour.Method.Name);
                behaviourExitFunctions[currentBehaviour.Method.Name]();
            }
            Debug.Log("Setting next behaviour to " + nextBehaviour.Method.Name + ".");
            currentBehaviour = nextBehaviour;
            hasNextBehaviour = false;
            if (behaviourEnterFunctions.ContainsKey(currentBehaviour.Method.Name)) {
                Debug.Log("Calling enter function for " + currentBehaviour.Method.Name);
                behaviourEnterFunctions[currentBehaviour.Method.Name]();
            }
        }

        void Update() {
            if (!hasIdleBehaviour) {
                Debug.LogError("No idle behaviour set");
                return;
            }
            if (waitingForAnimation) {
                if (waitingForAnimationState.IsName(waitingForAnimationName)) {
                    return;
                } else {
                    waitingForAnimation = false;
                    Debug.Log("Animation "+ waitingForAnimationName +" finished");
                }
            }
            BehaviourStatus behaviourStatus = currentBehaviour(this);
            currentBehaviourName = currentBehaviour.Method.Name;
            switch (behaviourStatus) {
                case BehaviourStatus.Continue:
                    break;
                case BehaviourStatus.ReturnToIdle:
                    nextBehaviour = idleBehaviour;
                    Debug.Log("Returning to idle");
                    ChangeBehaviour();
                    break;
                case BehaviourStatus.GoToNextBehaviour:
                    if (hasNextBehaviour) {
                        ChangeBehaviour();
                    } else {
                        Debug.LogError("No next behaviour set.");
                    }
                    break;
            }
        }
    }
}