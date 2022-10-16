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
        [SerializeField]
        private Sprite repairedSprite;
        [SerializeField]
        private Sprite brokenSprite;

        private SpriteRenderer spriteRenderer;
        public bool IsBroken {get; private set;} = false;

        void Awake() {
            playerChar.GetComponent<PlayerLogic>().RegisterSail();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void Repair() {
            Debug.Log("Sails repaired!");
            IsBroken = false;
            spriteRenderer.sprite = repairedSprite;
        }

        public void Break() {
            Debug.Log("Sails broken!");
            IsBroken = true;
            playerChar.GetComponent<PlayerLogic>().SailBreaks();
            spriteRenderer.sprite = brokenSprite;
        }

    }
}
