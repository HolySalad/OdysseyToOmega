using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraScript : MonoBehaviour
{
    // Start is called before the first frame update
    public void Shout(){
        FindObjectOfType<SoundManager>().Play("Hydra");
    }

    public void HydraAppear(){
        Debug.Log("Yeah this works");
        GetComponent<Animator>().SetTrigger("Appear");
    }
}
