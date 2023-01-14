using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship {    
    public class LadderActivatable : MonoBehaviour, IActivatables
    {
        [SerializeField] private GameObject ladderTop;
        [SerializeField] private GameObject ladderBottom;

        public ActivatablesNames kind {get;} = ActivatablesNames.Ladder;
        public bool isInUse {get; private set;} = false;
        public bool canManuallyDeactivate {get;} = true;
        public PlayerStateName playerState {get;} = PlayerStateName.ladder; 
        public string usageAnimation {get;} = "OnLadder";
        public string usageSound {get;} = "LadderClimb";

        private Player player;

        public (Vector2, float) LadderDirection() {
            Vector3 direction = ladderTop.transform.position - ladderBottom.transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            return (new Vector2(direction.x, direction.y), angle);
        }

        public bool ExitInContactWithPlayer(Player player) {
            return Physics2D.OverlapBox(ladderTop.transform.position, ladderTop.GetComponent<BoxCollider2D>().size, 0, LayerMask.GetMask("PlayerChar"));
        }

        public bool EntranceInContactWithPlayer(Player player) {
            return Physics2D.OverlapBox(ladderBottom.transform.position, ladderBottom.GetComponent<BoxCollider2D>().size, 0, LayerMask.GetMask("PlayerChar"));
        }

        public void Activate(Player player) {
            this.player = player;
            isInUse = true;
        }
        public void Deactivate(Player player) {
            isInUse = false;
            this.player = null;
        }
        public bool ActivationCondition(Player player) {
            return player;
        }
    }
}