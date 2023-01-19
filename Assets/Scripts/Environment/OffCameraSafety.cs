using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Environment {
    public class OffCameraSafety : MonoBehaviour
    {
        void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.GetComponent<Player>() != null)
            {
                Player player = other.gameObject.GetComponent<Player>();
                Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), true);
                Debug.Log("Player " + player.name + " has left the camera bounds.");
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.GetComponent<Player>() != null)
            {
                Player player = other.gameObject.GetComponent<Player>();
                if (player.currentPlayerStateName != PlayerStateName.hitstun) Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerChar"), LayerMask.NameToLayer("PhysicalHazards"), false);
                Debug.Log("Player " + player.name + " has entered the camera bounds.");
            }
        }
    }
}
