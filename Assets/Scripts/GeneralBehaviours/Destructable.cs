using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// a class for allowing objects to be destroyed while playing a correct sound and animation;
// just use Destroy() method to destroy objects silently when aniamtion or sound is unncessary.

namespace SpaceBoat {
    public class Destructable : MonoBehaviour
    {
        [SerializeField] private int health = 1;
        [SerializeField] private AnimationClip destructionAnimation;
        [SerializeField] private string destructionSound;

        public void Destruct() {
            Debug.Log("Destructing " + gameObject.name);
            float delay = 0f;
            if (destructionAnimation != null) {
                delay += destructionAnimation.length;
                GameModel.Instance.PlayAnimation(destructionAnimation, this.gameObject);
            }
            if (destructionSound != null && destructionSound != "") {
                SoundManager.Instance.Play(destructionSound);
            }
            MonoBehaviour[] behaviours;
            behaviours = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours) {
                behaviour.StopAllCoroutines();
            }
            Destroy(this.gameObject, delay);
        }

        public void Destruct(GameObject parent) {
            Debug.Log("Destructing " + gameObject.name);
            float delay = 0f;
            if (destructionAnimation != null) {
                delay += destructionAnimation.length;
                GameModel.Instance.PlayAnimation(destructionAnimation, this.gameObject);
            }
            if (destructionSound != null && destructionSound != "") {
                SoundManager.Instance.Play(destructionSound);
            }
            MonoBehaviour[] behaviours;
            behaviours = parent.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour behaviour in behaviours) {
                behaviour.StopAllCoroutines();
            }
            Destroy(parent, delay);
        }

        private int hp;

        public void Start() {
            hp = health;
        }

        public void Damage() {
            hp --;
            if (hp <= 0) {
                Destruct();
            }
        }
    }
}