using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SpaceBoat.Items;
namespace SpaceBoat.Rewards {
    public class RewardComet : MonoBehaviour
    {
        [SerializeField] private GameObject itemPlace;
        [SerializeField] private float speed = 1f;
        [SerializeField] private bool LaunchOnStart = false;
        
        [SerializeField] public ItemTypes containedItemType;
        public int numItems;
        public IHeldItems containedItem;

        private Destructable destructable;

        //TODO find a nicer way of doing below
        private float destructionYRight = -2.00f;
        private float destructionYLeft = 5f;
        private float destructionLeftRightBoundryX = -6f;
        private bool beingDestroyed = false;


        public void Start() {
            destructable = GetComponent<Destructable>();
            if (containedItemType != ItemTypes.None && containedItem == null) {
               SetItemType(containedItemType);
            }
            if (LaunchOnStart) {
                LaunchComet();
            }
        }

        public void LaunchComet(bool isPity) {
            Vector3 target = GameModel.Instance.cometFlightTarget.transform.position;
            if (isPity) {
                target = GameModel.Instance.cometDeckTarget.transform.position;
            }
            Vector3 targetVector = target - transform.position;
            GetComponent<Rigidbody2D>().velocity = targetVector.normalized * speed;
        }

        public void LaunchComet() {LaunchComet(false);}

        public void SetItemType(ItemTypes itemType) {
            containedItemType = itemType;
            GameObject Prefab = GameModel.Instance.PrefabForItemType(itemType);
            if (Prefab == null) {
                Debug.LogError("RewardComet: No prefab for item type " + itemType);
                return;
            }
            GameObject item = Instantiate(Prefab, itemPlace.transform.position, Quaternion.identity);
            SpriteRenderer itemRenderer = itemPlace.GetComponent<SpriteRenderer>();
            itemRenderer.sprite = item.GetComponent<SpriteRenderer>().sprite;
            itemRenderer.transform.localScale = new Vector3(1, 1, 1);
            Destroy(item);
            containedItem = GameModel.Instance.CreateItemComponent(this.gameObject, itemType);
        }

        void DropItemAndDestruct() {
            beingDestroyed = true;
            for (int i = 0; i < numItems; i++) {
                Vector3 position = transform.position + new Vector3(Mathf.Floor((numItems - i)*3 - numItems), 0, 0);
                GameObject item = Instantiate(GameModel.Instance.PrefabForItemType(containedItemType), position, Quaternion.identity);
                GameModel.Instance.CreateItemComponent(item, containedItemType);
            }
            destructable.Destruct();
        }

        //this odesn't work becuas the ship is static
        //fixing it is a bitch.
        public void OnColliderEnter2D(Collider2D other) {
            if (other.gameObject.CompareTag("Ship")) {
                DropItemAndDestruct();
            }
        }

        public void Update() {
            if (beingDestroyed) return;
            if ((transform.position.x < destructionLeftRightBoundryX && transform.position.y < destructionYLeft)
            || (transform.position.x > destructionLeftRightBoundryX && transform.position.y < destructionYRight)) {
                DropItemAndDestruct();
            }       
        }
    }
}