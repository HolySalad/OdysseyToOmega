using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerSubclasses.PlayerStates {
    public class HitstunState : MonoBehaviour, IPlayerState
    {
        public bool stealVelocityControl {get;} = false;

        private float timeEnteredState = 0;
        private Player player;
        
        [SerializeField] private float hitStunTime = 1;

        void Awake() {
            player = GetComponent<Player>();
        }

        public void EnterState(PlayerStateName previousState) {
            timeEnteredState = Time.time;
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), true);
        }
        public void ExitState(PlayerStateName nextState) {
            Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), false);
        }
        public void UpdateState() {
            if (Time.time - timeEnteredState > hitStunTime) {
                player.ChangeState(PlayerStateName.ready);
                return;
            }
            player.WalkInput(0f); // input 0 for walk movement to decelerate the player naturally.
        }
    }
}