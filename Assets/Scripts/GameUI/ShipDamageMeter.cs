using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBoat.UI {
    public class ShipDamageMeter : MonoBehaviour
    {
        [SerializeField] private Sprite[] shipHealthSprites;
        [SerializeField] private float flickeringTime = 1f;
        [SerializeField] private float flickeringSpeed = 0.05f;

        private Image healthBar;
        private int lastHealth = 0;
        private int currentHealth = 0;

        IEnumerator FlickerHealthBar() {
            float flickerTimer = 0f;
            bool flickerState = false;
            while (flickerTimer < flickeringTime) {
                flickerTimer += flickeringSpeed;
                if (flickerState) {
                    healthBar.sprite = shipHealthSprites[currentHealth];
                } else {
                    healthBar.sprite = shipHealthSprites[lastHealth];
                }
                flickerState = !flickerState;
                yield return new WaitForSeconds(flickeringSpeed);
            }
            healthBar.sprite = shipHealthSprites[currentHealth];
            lastHealth = currentHealth;
        }

        void OnGUI() {
            int health = GameModel.Instance.lastSurvivingSailCount;
            if (healthBar == null) {
                healthBar = GetComponent<Image>();
                currentHealth = health;
                lastHealth = health;
                healthBar.sprite = shipHealthSprites[health];
            }
            if (currentHealth != health) {
                StopAllCoroutines();
                currentHealth = health;
                StartCoroutine(FlickerHealthBar());
            }
        }
    }
}