using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;

    public static SoundManager Instance;

    //TODO Add to awake FindObjectOfType<SoundManager>() and then call it and ad .("WhatheverSoundName")
    void Awake()
    {
        if (Instance != null){
            Debug.Log(Instance.name);
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
    }

    // Update is called once per frame
    public void Play(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        Debug.Log("Playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
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
        return s.source.isPlaying;
    }
}
