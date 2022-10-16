using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat {
    public class HarpoonGun : MonoBehaviour
    {

        //on awake, reverse the gun and hide the harpoon.
        void Awake() {
            
            transform.GetChild(1).gameObject.GetComponent<SpriteRenderer>().enabled = false;
        
        }


    }
}