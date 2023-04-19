using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Enemies.Helpers;
using SpaceBoat.HazardManagers;
using SpaceBoat.HazardManagers.ChydraBossSubclasses;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D.IK;

//contains functions which are called by animation events.


namespace SpaceBoat.Enemies.ChydraNew {
    public class ChydraController : MonoBehaviour
    {
        [Header("Object References")]
        [SerializeField] private GameObject jawAnchor;
        [SerializeField] private LimbSolver2D jawSolver;
        [SerializeField] private Transform bombardmentTargetSolver;
        private ChydraBoss chydraBoss;
        private Animator animator;
        public bool isActivated = false;

        public void HeadDeathTriggers(bool head2, bool head3, bool completelyDead) {
            animator.SetBool("Dead", completelyDead);
            animator.SetBool("H2Awaken", head2);
            animator.SetBool("H3Awaken", head3);
            animator.SetBool("CompletelyDead", completelyDead);
            
        }
        
        // attacks 
        private GameObject dummyCharge;

        public void StartFireballCharge() {
            Debug.Log("starting fireball charge"); //+ new Vector3(-4, -6, 0)
            if (dummyCharge == null) {
                dummyCharge = Instantiate(chydraBoss.fireballPrefab, jawAnchor.transform);
                dummyCharge.GetComponent<Fireball>().enabled = false;
                foreach (Light2D light in dummyCharge.GetComponentsInChildren<Light2D>()) {
                    light.enabled = false;
                }
                dummyCharge.transform.localRotation = Quaternion.Euler(0, 0, -121.6f);
                dummyCharge.transform.localScale = new Vector3(1, 1, 1);
                dummyCharge.GetComponent<SpriteRenderer>().enabled = false;
            }
            dummyCharge.GetComponent<SpriteRenderer>().enabled = true;
            dummyCharge.transform.localPosition = new Vector3(2.4f, 0.1f, 0);
        }

        public void LaunchFireball() {
            Debug.Log("launching fireball");
            Vector3 direction = ((GameModel.Instance.player.transform.position+new Vector3(0, Random.Range(-2, 2), 0)) - jawAnchor.transform.position).normalized;
            GameObject Fireball = Instantiate(chydraBoss.fireballPrefab, jawAnchor.transform.position , Quaternion.identity);
            Fireball currentFireball = Fireball.GetComponent<Fireball>();
            currentFireball.Launch(direction);
            dummyCharge.GetComponent<SpriteRenderer>().enabled = false;
        }

        public void LaunchAcidAttack() {
            Debug.Log("Pew");
        }

        // limb solver


        float targetSolverWeight = 0f;
        private Transform limbSolverDefault;
        IEnumerator UpdateLimbSolver() {
            while (true) {
                yield return null;
                if (jawSolver.weight < targetSolverWeight) {
                    jawSolver.weight = Mathf.Min(jawSolver.weight + Time.deltaTime, targetSolverWeight);
                } else if (jawSolver.weight > targetSolverWeight) {
                    jawSolver.weight = Mathf.Max(jawSolver.weight - Time.deltaTime, targetSolverWeight);
                }
            }
        }
        public enum SolverTargetSettings {
            Default,
            Player,
            Bombardment
        }

        public void SetSolverTarget(SolverTargetSettings setting) {
            if (jawSolver.weight != 0) {
                Debug.Log("Solver weight is not 0, but the solver target is being changed");
                Debug.Log("Solver target should only be changed when exiting the idle state. If the weight hasn't reached 0 when exiting the idle state, the idle time is too low.");
            }
            switch (setting) {
                case SolverTargetSettings.Default:
                    jawSolver.GetChain(0).target = limbSolverDefault;
                    break;
                case SolverTargetSettings.Player:
                    jawSolver.GetChain(0).target = GameModel.Instance.player.transform;
                    break;
                case SolverTargetSettings.Bombardment:
                    jawSolver.GetChain(0).target = bombardmentTargetSolver;
                    break;
            }
        }

        public void SetSolverWeightTarget(float targetWeight) {
            targetSolverWeight = targetWeight;
        }

        public void ActivateController(ChydraBoss chydraBoss) {
            isActivated = true;
            this.chydraBoss = chydraBoss;
            limbSolverDefault = jawSolver.GetChain(0).target;
            jawSolver.weight = 0;
            jawSolver.GetChain(0).target = GameModel.Instance.player.transform;
            StartCoroutine(UpdateLimbSolver());
            animator = GetComponent<Animator>(); 
        }
    }
}