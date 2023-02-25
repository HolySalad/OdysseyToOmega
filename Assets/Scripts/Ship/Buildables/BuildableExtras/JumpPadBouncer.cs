using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Ship.Buildables.BuildableExtras {
    public class JumpPadBouncer : MonoBehaviour, Environment.IBouncable
    {
        public bool Bounce(Player player) {
            float playerVelocity = player.gameObject.GetComponent<Rigidbody2D>().velocity.y;
            if (playerVelocity > 0) {
                return false;
            }
            Debug.Log("Jumppad bounce");
            SoundManager.Instance.Play("JumpPadBounce");
            player.ForceJump(false, true, true, 2.6f);

            return true;
        }
    }
}