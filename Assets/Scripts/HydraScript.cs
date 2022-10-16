using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HydraScript : MonoBehaviour
{
    // Start is called before the first frame update
    public void Shout(){
        FindObjectOfType<SoundManager>().Play("Hydra");
        StartCoroutine("ToBeContinued");
    }

    public void HydraAppear(){
        Debug.Log("Yeah this works");
        GetComponent<Animator>().SetTrigger("Appear");
    }

    private IEnumerator ToBeContinuedScreen(){
        yield return new WaitForSeconds(2.8f);
        SceneManager.LoadScene("ToBeContinued");
    }
}
