using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBoat.UI {    
    public class MoneyCounter : MonoBehaviour
    {
        [SerializeField] private bool isMenu = false;
        [SerializeField] private float moneyTickTime = 0.2f;
        [SerializeField] private float moneyTickFastThreshold = 20;
        [SerializeField] private float moneyTickFastTime = 0.05f;
        [SerializeField] private float fadeOutTime = 4f;
        [SerializeField] private float fadeOutSpeed = 0.8f;
        [SerializeField] private float fadeInSpeed = 0.2f;
        [SerializeField] private Image moneyIcon;
        private int lastDisplayedMoney = 0;

        void Start() {
            TMPro.TextMeshProUGUI text = GetComponent<TMPro.TextMeshProUGUI>();
            lastDisplayedMoney = GameModel.Instance.player.money;
            text.text = lastDisplayedMoney.ToString();
            if (isMenu) return;
            text.color = new Color(text.color.r, text.color.g, text.color.b, 0);
            moneyIcon.color = new Color(moneyIcon.color.r, moneyIcon.color.g, moneyIcon.color.b, 0);
        }

        IEnumerator TickMoneyUp(int money) {
            int moneyToDisplay = lastDisplayedMoney;
            TMPro.TextMeshProUGUI text = GetComponent<TMPro.TextMeshProUGUI>();

            if (text.color.a < 1) {
                while (text.color.a < 1) {
                    Color col = new Color(text.color.r, text.color.g, text.color.b, Mathf.Min(text.color.a + (Time.unscaledDeltaTime * fadeInSpeed), 1));
                    text.color = col;
                    moneyIcon.color = col;
                    yield return null;
                }
            }
            while (moneyToDisplay != money) {
                if (moneyToDisplay < money) moneyToDisplay++;
                    else moneyToDisplay--;

                text.text = moneyToDisplay.ToString();

                if (Mathf.Abs(money - moneyToDisplay) > moneyTickFastThreshold) {
                    yield return new WaitForSecondsRealtime(moneyTickFastTime);
                } else 
                    yield return new WaitForSecondsRealtime(moneyTickTime);
            }
            lastDisplayedMoney = money;
            if (GameModel.Instance.isPaused) yield break;
            yield return new WaitForSecondsRealtime(fadeOutTime);
            while (text.color.a > 0) {
                Color col = new Color(text.color.r, text.color.g, text.color.b, Mathf.Max(text.color.a - (Time.unscaledDeltaTime * fadeOutSpeed), 0));
                text.color = col;
                moneyIcon.color = col;
                yield return null;
            }

        }

        public void OnGUI() {
            Player player = GameModel.Instance.player;
            if (player == null) return;
            int money = player.money;
            if (isMenu) {
                TMPro.TextMeshProUGUI text = GetComponent<TMPro.TextMeshProUGUI>();
                text.text = money.ToString();
                return;
            }
            if (GameModel.Instance.gameOverTriggered) {
                StartCoroutine(TickMoneyUp(0));
                return;
            }
            if (money != lastDisplayedMoney) {
                StopCoroutine(TickMoneyUp(money));
                StartCoroutine(TickMoneyUp(money));
            }
            if (GameModel.Instance.isPaused){ 
                moneyIcon.color = new Color(moneyIcon.color.r, moneyIcon.color.g, moneyIcon.color.b, 1);
                TMPro.TextMeshProUGUI text = GetComponent<TMPro.TextMeshProUGUI>();
                text.color = new Color(text.color.r, text.color.g, text.color.b, 1);
            }
        }
    }
}