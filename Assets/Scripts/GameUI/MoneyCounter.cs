using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBoat.UI {    
    public class MoneyCounter : MonoBehaviour
    {
        public void OnGUI() {
            Player player = GameModel.Instance.player;
            if (player == null) return;
            int money = player.money;
            GetComponent<TMPro.TextMeshProUGUI>().text = money.ToString();
        }
    }
}