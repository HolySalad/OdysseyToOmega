using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceBoat.Rewards;
using UnityEngine.SceneManagement;

namespace SpaceBoat.UI {
    public class PauseUI : MonoBehaviour
    {
        private enum PauseMenuState {
            Menu,
            Options
        }
        private PauseMenuState currentState;
        private GameModel game;

        [SerializeField] private GameObject menuParent;
        [SerializeField] private GameObject optionsParent;

        [SerializeField] Slider generalVolumeSlider;
        [SerializeField] Slider musicVolumeSlider;
        [SerializeField] Slider effectsVolumeSlider;

        void Awake(){
            game = FindObjectOfType<GameModel>();
        }
        // Start is called before the first frame update
        void Start()
        {
            currentState = PauseMenuState.Menu;
            menuParent.SetActive(true);
            SetSliders();
        }

        // Update is called once per frame
        void Update() {
                if (Input.GetKeyDown(KeyCode.Escape)) {
                    if(currentState == PauseMenuState.Menu)
                        Resume();
                    else if(currentState == PauseMenuState.Options)
                        CloseOptions();
                }
            }

        public void Resume(){
            UIManager.Instance.ClosePauseMenu();
        }

        public void SaveExit(){
            game.saveGameManager.Save();
            GameModel.Instance.UnpauseGame();
            SceneManager.LoadScene("OdysseyMainMenu");
        }
        public void ShowOptions(){
            currentState = PauseMenuState.Options;
            SetSliders();
            menuParent.SetActive(false);
            optionsParent.SetActive(true);
        }
        public void CloseOptions(){
            currentState = PauseMenuState.Menu;
            optionsParent.SetActive(false);
            menuParent.SetActive(true);
        }

        public void SetMusicVolume(float newVolume){
            SoundManager.Instance.musicVolume = newVolume;
            SoundManager.Instance.SetMusicVolume(newVolume);
        }public void SetSFXVolume(float newVolume){
            SoundManager.Instance.sfxVolume = newVolume;
            SoundManager.Instance.SetSFXVolume(newVolume);
        }public void SetVolume(float newVolume){
            SoundManager.Instance.masterVolume = newVolume;
            SoundManager.Instance.SetMasterVolume(newVolume);
        }

        public void SetSliders(){
            generalVolumeSlider.value = SoundManager.Instance.masterVolume;
            musicVolumeSlider.value = SoundManager.Instance.musicVolume;
            effectsVolumeSlider.value = SoundManager.Instance.sfxVolume;
        }
    }
}
