using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceBoat.UI
{    public class HeartMeter : MonoBehaviour
    {
        [SerializeField] private Sprite heartFull;
        [SerializeField] private Sprite heartEmpty;

        [SerializeField] private int value;

        public void OnGUI() {
            Player player = GameModel.Instance.player;
            int health = player.health;
            if (health >= value) {
                GetComponent<Image>().sprite = heartFull;
            } else {
                GetComponent<Image>().sprite = heartEmpty;
            }
        }
    }
}