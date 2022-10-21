using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceBoat.UI {
    public class HPUIManager : MonoBehaviour
    {
        [SerializeField] private GameObject[] hearts;
        [SerializeField] private Sprite fullHeart;
        [SerializeField] private Sprite emptyHeart;
        // Start is called before the first frame update
        public void SetHP(int hp) {
            foreach(GameObject heart in hearts){
                int heartNum = 1;
                Debug.Log("Deciding whether to show heart " + heartNum + "when the player is at " + hp + " health");
                if(heartNum >= hp)
                {
                    heart.GetComponent<SpriteRenderer>().sprite = fullHeart;
                }
                else
                {
                    heart.GetComponent<SpriteRenderer>().sprite = emptyHeart;
                }
                heartNum++;
            }
        }
    }
}