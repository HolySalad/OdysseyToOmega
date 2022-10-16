using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SoundManager : MonoBehaviour
{
    public Sound[] sounds;

    public static SoundManager instance;

    //TODO Add to awake FindObjectOfType<SoundManager>() and then call it and ad .("WhatheverSoundName")
    void Awake()
    {
        if (instance != null){
            Debug.Log(instance.name);
            Destroy(gameObject);
            return;
        }
        else{
            
            instance = this;
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

    public void Stop(string name){
        Sound s = Array.Find(sounds, sound => sound.name == name);
        Debug.Log("No longer playing Sound " + name);
        if(s == null){
            Debug.LogWarning("Sound "+ name + " not found.");
            return;
        }
        s.source.Stop();
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
