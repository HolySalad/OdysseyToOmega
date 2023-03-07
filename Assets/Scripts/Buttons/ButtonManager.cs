using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    private Animator animator;
    [SerializeField] GameObject OptionsPanel;
    [SerializeField] GameObject SoundPanel;
    [SerializeField] GameObject CreditsPanel;

    [SerializeField] Slider generalVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider effectsVolumeSlider;

    private bool soundActive = false;
    private bool creditsActive = false;

    //Add to awake FindObjectOfType<SoundManager>() and then call it and add .("WhatheverSoundName")
    private void Start(){
        animator = GetComponent<Animator>();
        //SoundPanel.SetActive(false);
        if (VariableManager.Instance != null) {
            SetSliderDefaults();
        }
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
            VariableManager.Instance.SaveSettings();
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
       // SoundPanel.SetActive(false);
    }

    public void SetMusicVolume(float newVolume){
        VariableManager.Instance.MusicVolume = newVolume;
    }public void SetSFXVolume(float newVolume){
        VariableManager.Instance.EffectsVolume = newVolume;
    }public void SetVolume(float newVolume){
        VariableManager.Instance.GeneralVolume = newVolume;
    }

    public void SetSliderDefaults(){
        generalVolumeSlider.value = VariableManager.Instance.GeneralVolume;
        musicVolumeSlider.value = VariableManager.Instance.MusicVolume;
        effectsVolumeSlider.value = VariableManager.Instance.EffectsVolume;
    }

    public void ResetGameButton(){
        VariableManager.Instance.resetGame = true;
    }
}
