using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(BackToMenuCoroutine());
    }

    IEnumerator BackToMenuCoroutine(){
        yield return new WaitForSeconds(2f);
        SoundManager sm = SoundManager.Instance;
        SceneManager.LoadScene("MainMenu");
    }
}
