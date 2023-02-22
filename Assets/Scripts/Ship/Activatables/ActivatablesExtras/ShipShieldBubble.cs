using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SpaceBoat.Ship.Activatables 
{    public class ShipShieldBubble : MonoBehaviour
    {
        void OnTriggerEnter2D(Collider2D other) {
            if (other.gameObject.layer == LayerMask.NameToLayer("PhysicalHazards") || other.gameObject.layer == LayerMask.NameToLayer("MomentumHazards")) {
                if (other.gameObject.GetComponent<Destructable>() != null) {
                    other.gameObject.GetComponent<Destructable>().Destruct();
                } else {
                    Destroy(other.gameObject);
                }
            }
        }
    }
}