using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.Ship {
    public class SailsActivatable : MonoBehaviour
    {
        [SerializeField] private Sprite repairedSprite;
        [SerializeField] private Sprite brokenSprite;

        private SpriteRenderer spriteRenderer;
        public bool isBroken {get; private set;} = false;

        void Awake() {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (repairedSprite == null) {
                Debug.LogError("Sails: No repaired sprite set on "+ this.gameObject.name);
            }
            if (brokenSprite == null) {
                Debug.LogError("Sails: No broken sprite set on "+ this.gameObject.name);
            }
        }

        public void Repair() {
            isBroken = false;
            spriteRenderer.sprite = repairedSprite;
        }

        public void Break() {
            isBroken = true;
            spriteRenderer.sprite = brokenSprite;
        }

    }
}