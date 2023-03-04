using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonManager : MonoBehaviour
{
    //TODO reset button functionality
    private Animator animator;
    [SerializeField] GameObject OptionsPanel;
    [SerializeField] GameObject SoundPanel;
    [SerializeField] GameObject CreditsPanel;
    private bool soundActive = false;
    private bool creditsActive = false;

    //Add to awake FindObjectOfType<SoundManager>() and then call it and ad .("WhatheverSoundName")
    private void Start(){
        animator = GetComponent<Animator>();
        SoundPanel.SetActive(false);
    }

    #region Buttons
    public void CreditsButton(){
        if(creditsActive){
            animator.SetTrigger("GetCreditsOut");
            creditsActive = false;
        }else{
            animator.SetTrigger("GetCreditsIn");
            creditsActive = true;
        }
    }

    public void MainMenuButton(){
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayButton(){
        SceneManager.LoadScene("TheBoatoType");
    }

    public void ExitButton(){
        Application.Quit();
        Debug.Log("You quit");
    }

    public void OptionsButton(){
        animator.SetTrigger("GetOptionsIn");
    }
    public void CloseOptionsButton(){
        animator.SetTrigger("GetOptionsOut");
    }

    public void SoundButton(){
        if(soundActive){
            soundActive = false;
            animator.SetTrigger("GetSoundOut");
            StartCoroutine("DisableSound");
        }else{
            SoundPanel.SetActive(true);
            soundActive = true;
            StopCoroutine("DisableSound");
            animator.SetTrigger("GetSoundIn");
        }
    }
    #endregion

    #region Totem toggling
    private bool totemActive = false;
    public void TotemButton(){
        if(totemActive){
            animator.SetTrigger("GetTotemOut");
            totemActive = false;
        }else{
            animator.SetTrigger("GetTotemIn");
            totemActive = true;
        }
    }
    #endregion

    private IEnumerator DisableSound(){
        yield return new WaitForSeconds(0.4f);
        SoundPanel.SetActive(false);
    }

    public void SetMusicVolume(float newVolume){

    }public void SetSFXVolume(float newVolume){
        
    }public void SetVolume(float newVolume){
        
    }
}
