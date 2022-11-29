using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat;

namespace SpaceBoat.Environment {    
    public class OutOfBounds : MonoBehaviour
    {
        public void OnCollisionEnter2D(Collision2D collision) {
            if (collision.gameObject.tag == "Player") {
                collision.gameObject.GetComponent<Player>().PlayerDies(true);
            }
        }
    }
}