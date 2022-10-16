using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{

    //Add to awake FindObjectOfType<SoundManager>() and then call it and ad .("WhatheverSoundName")
    public void CreditsButton(){
        SceneManager.LoadScene("Credits");
    }

    public void MainMenuButton(){
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayButton(){
        SceneManager.LoadScene("TheJourney");
    }

    public void ExitButton(){
        Application.Quit();
        Debug.Log("You quit");
    }
}
