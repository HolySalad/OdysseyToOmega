using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TotemEntities;

namespace SpaceBoat {
    [System.Serializable] public class TotemAvatarHead {
        public SpriteRenderer head;
    }
    public class TotemAvatars : MonoBehaviour
    {
        [SerializeField] private List<TotemAvatarHead> totemAvatarHeads = new List<TotemAvatarHead>();
        [SerializeField] private SpriteRenderer defaultHead;

        private SpriteRenderer currentHead;
        private SpriteRenderer CthulkObject;
        private bool isScriptInUI = false;

        void Awake() {
            currentHead = defaultHead;
            CthulkObject = GetComponent<SpriteRenderer>();
            if (GetComponent<Player>() == null) {
                isScriptInUI = true;
            }
        }

        public void SetAvatar(TotemAvatarHead totemAvatarHead) {
            currentHead.enabled = false;
            currentHead = totemAvatarHead.head;
            currentHead.enabled = true;
        }

        public void SetColour(Color colour) {
            currentHead.color = colour;
        }
    }
}