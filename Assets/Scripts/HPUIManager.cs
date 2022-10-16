using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPUIManager : MonoBehaviour
{
    [SerializeField] private GameObject[] ListOfFullHP;
    [SerializeField] private GameObject[] ListOfNoHP;
    // Start is called before the first frame update
    public void LooseHP()
    {
        int actualHeart = 0;
        foreach(GameObject heart in ListOfFullHP){
            if(heart.activeInHierarchy == true){
                heart.SetActive(false);
                ListOfNoHP[actualHeart].SetActive(true);
                return;
            }
            else{
                actualHeart++;
            }
        }
    }
    public void GainHP()
    {
        int actualHeart = 0;
        foreach(GameObject heart in ListOfNoHP){
            if(heart.activeInHierarchy == true){
                heart.SetActive(false);
                ListOfFullHP[actualHeart].SetActive(true);
                return;
            }else{
                actualHeart++;
            }
        }
    }
}
