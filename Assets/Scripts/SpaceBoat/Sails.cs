using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Player;

namespace SpaceBoat
{
    public class Sails : MonoBehaviour
    {
        [SerializeField]
        private GameObject playerChar;
        public bool IsBroken {get; private set;} = false;

        void Awake() {
            playerChar.GetComponent<PlayerLogic>().RegisterSail();
        }

        public void Repair() {
            Debug.Log("Sails repaired!");
            IsBroken = false;
        }

        public void Break() {
            Debug.Log("Sails broken!");
            IsBroken = true;
            playerChar.GetComponent<PlayerLogic>().SailBreaks();
        }

    }
}
