using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.PlayerStates {
    public class WorkingState : MonoBehaviour, IPlayerState
    {
        public bool stealInput {get;}

        private int frameEnteredState = 0;
        private bool finishedUsage = false;
        private Player player;
        

        void Awake() {
            player = GetComponent<Player>();
        }

        public void EnterState(PlayerStateName previousState) {
           frameEnteredState = Time.frameCount;
           finishedUsage = false;
        }
        public void ExitState(PlayerStateName nextState) {
            if (!finishedUsage) {
                player.CancelItemUsage();
            }
        }
        public void UpdateState() {
            // if item usage is finished, exit the state.
            if (Time.frameCount > frameEnteredState + player.itemInHand.usageFrames) {
                player.CompleteItemUsage();
                finishedUsage = true;
                player.ChangeState(PlayerStateName.ready);
                return;
            }
            // check if the player is still in contact with the item usage target.
            if (!player.itemUsageTarget.GetComponent<Collider2D>().OverlapPoint(player.itemPlace.position)) {
                Debug.Log("Player left the item usage target collider: " + player.itemUsageTarget.name);
                player.CancelItemUsage();
                finishedUsage = false;
                player.ChangeState(PlayerStateName.ready);
                return;
            }


            player.WalkInput(0f); // input 0 for walk movement to decelerate the player naturally.
            player.ItemUsageInput(CthulkInput.UseItemDown());
            // note, if we add more logic here, we should return if the above function returns true.
        }
    }
}