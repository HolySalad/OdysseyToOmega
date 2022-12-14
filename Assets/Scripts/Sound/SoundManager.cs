using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;

    public static SoundManager Instance;

    public delegate void afterSoundCallback();

    private List<Coroutine> coroutines = new List<Coroutine>();

    //TODO Add to awake FindObjectOfType<SoundManager>() and then call it and ad .("WhatheverSoundName")
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
        DontDestroyOnLoad(gameObject); 


        foreach (Sound sound in sounds){
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;

            sound.source.volume = sound.volume;
            sound.source.pitch = 1;
            sound.source.loop = sound.loop;
        }

        SceneManager.activeSceneChanged += ClearSoundsOnSceneChange;
    }

    void RefreshSources(){
        Debug.Log("A AudioSource was null, refreshing them!");
        foreach (Sound sound in sounds){
            if(sound.source == null) {
                sound.source = gameObject.AddComponent<AudioSource>();
                sound.source.clip = sound.clip;

                sound.source.volume = sound.volume;
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
    public void Play(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        Debug.Log("Playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        if (s.source == null) RefreshSources();
        s.source.Play();
    }

    public void Play(string name, afterSoundCallback callback){
        Sound s = Array.Find(sounds, sound => sound.name == name);
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

    public void Play(string name, float volume){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        Debug.Log("Playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        
        s.source.volume = volume;
        s.source.Play();
    }

    public void Oneshot(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        Debug.Log("Playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        s.source.PlayOneShot(s.source.clip);
    }

    public void Stop(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        Debug.Log("No longer playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        if (s.source == null) return;
        s.source.Stop();
    }

    public float Length(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        Debug.Log("Getting length of " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return 0f;
        }
        return s.clip.length;
    }

    public bool IsPlaying(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return false;
        }
        if (s.source == null) RefreshSources();
        return s.source.isPlaying;
    }
}
