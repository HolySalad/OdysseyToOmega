using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    public float secondsToWait = 6f;
    // Start is called before the first frame update
    void Awake()
    {

        Debug.Log("Im trying");
        StartCoroutine("LoadNextScene");
    }

    private IEnumerator LoadNextScene(){
        yield return new WaitForSeconds(secondsToWait);
        SceneManager.LoadScene("MainMenu");
    }
}
