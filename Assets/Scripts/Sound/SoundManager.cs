using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    [Range(0f,1f)]
    [SerializeField] private float masterVolume = 1f;
    [Range(0f,1f)]
    [SerializeField] private float musicVolume = 1f;
    [Range(0f,1f)]
    [SerializeField] private float sfxVolume = 1f;
    public Sound[] sounds;
    public Dictionary<string, Sound> soundsDict = new Dictionary<string, Sound>();

    public static SoundManager Instance;

    public delegate void afterSoundCallback();

    private List<Coroutine> coroutines = new List<Coroutine>();

    //TODO Add to awake FindObjectOfType<SoundManager>() and then call it and ad .("WhatheverSoundName")

    void SetTrueVolume (Sound s, float volume) {
        if (s.isMusic) {
            s.source.volume = volume * masterVolume * musicVolume;
        } else {
            s.source.volume = volume * masterVolume * sfxVolume;
        }
    }

    void Awake()
    {
        if (Instance != null){
            Debug.Log("New SoundManager created but "+ Instance.name +" already exists, destroying new one");
            Destroy(gameObject);
            return;
        }
        else{
            Instance = this;
        }


        foreach (Sound sound in sounds){
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            SetTrueVolume(sound, sound.volume);
            sound.source.pitch = 1;
            sound.source.loop = sound.loop; 
            soundsDict.Add(sound.name, sound);
        }

        //SceneManager.activeSceneChanged += ClearSoundsOnSceneChange;
    }

    void RefreshSources(){
        Debug.Log("A AudioSource was null, refreshing them!");
        foreach (Sound sound in sounds){
            if(sound.source == null) {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;

                SetTrueVolume(sound, sound.volume);
                sound.source.pitch = 1;
                sound.source.loop = sound.loop;
            }
        }
    }

    void ClearSoundsOnSceneChange(Scene current, Scene next){
        foreach (Coroutine c in coroutines){
            StopCoroutine(c);
        }
        coroutines.Clear();
    }

    // Update is called once per frame
    public void Play(string name, afterSoundCallback callback){
        Sound s = soundsDict[name];
        Debug.Log("Playing Sound " + name + " with callback");
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        if (s.source == null) RefreshSources();
        coroutines.Add(StartCoroutine(CallbackAfterSound(s, callback)));
        s.source.Play();
    }

    IEnumerator CallbackAfterSound(Sound s, afterSoundCallback callback){
        yield return new WaitForSeconds(s.clip.length);
        callback();
    }

    IEnumerator FadeSoundIn(Sound s, float targetVolume, float time, float delayBeforeFade){
        if (delayBeforeFade > 0f) yield return new WaitForSeconds(delayBeforeFade);
        float startVolume = s.source.volume;
        float endVolume = targetVolume;
        float startTime = Time.time;
        float endTime = startTime + time;
        while (Time.time < endTime){
            SetTrueVolume(s, Mathf.Lerp(startVolume, endVolume, (Time.time - startTime) / time));
            yield return null;
        }
        SetTrueVolume(s, endVolume);
    }

    public void Play(string name, float volume = 1f, bool fadeIn = false, float fadeTime = 1f, float delayBeforeFade = 0f) {
        Sound s = soundsDict[name];
        Debug.Log("Playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        if (fadeIn) {
            s.source.volume = 0f;
            coroutines.Add(StartCoroutine(FadeSoundIn(s, volume, fadeTime, delayBeforeFade)));
        } else {
            SetTrueVolume(s, volume);
        }
        
        s.source.Play();
    }

    public void Oneshot(string name, float volume = 1f){
        Sound s = soundsDict[name];
        Debug.Log("Playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        float trueVolume = volume * masterVolume;
        if (s.isMusic) trueVolume *= musicVolume;
        else trueVolume *= sfxVolume;
        s.source.PlayOneShot(s.source.clip, trueVolume);
    }

    IEnumerator FadeOutSound(Sound s, float time){
        float startVolume = s.source.volume;
        float startTime = Time.time;
        float endTime = startTime + time;
        while (Time.time < endTime){
            s.source.volume = Mathf.Lerp(startVolume, 0f, (Time.time - startTime) / time);
            yield return null;
        }
        s.source.volume = 0f;
        s.source.Stop();
    }

    public void Stop(string name, bool fadeOut = false, float fadeTime = 1f){
        Sound s = soundsDict[name];
        Debug.Log("No longer playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        if (s.source == null) return;
        if (fadeOut) {
            coroutines.Add(StartCoroutine(FadeOutSound(s, fadeTime)));
        } else {
            s.source.Stop();
        }
    }

    public float Length(string name){
        Sound s = soundsDict[name];
        Debug.Log("Getting length of " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return 0f;
        }
        return s.clip.length;
    }

    public bool IsPlaying(string name){
        Sound s = soundsDict[name];
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return false;
        }
        if (s.source == null) RefreshSources();
        return s.source.isPlaying;
    }
}
