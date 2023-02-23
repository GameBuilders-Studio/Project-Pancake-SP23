using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SFXDictionary : SerializableDictionary<string, AudioClip>
{}

[RequireComponent(typeof(AudioSource))]
public class SFXManager : Singleton<SFXManager>
{
    public SFXDictionary sfxClips;
    private AudioSource audioSource;
    private float volume = 1;
    private static bool haveDone = false;
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }


    public void SetVolume(float num)
    {
        volume = num;
        audioSource.volume = volume;
    }
    public void Mute()
    {
        audioSource.mute = true;
    }
    public void UnMute()
    {
        audioSource.mute = false;
    }
    public float GetVolume()
    {
        return volume;
    }
    public static void PlayMusic(string SFXName)
    {
        if (Instance.sfxClips.ContainsKey(SFXName))
        {
            Instance.audioSource.clip = Instance.sfxClips[SFXName];
            Instance.audioSource.loop = false;
            Instance.audioSource.Play();
        }
    }

    public static void PlayMusicLoop(string SFXName)
    {
        if (Instance.sfxClips.ContainsKey(SFXName))
        {
            Instance.audioSource.clip = Instance.sfxClips[SFXName];
            Instance.audioSource.loop = true;
            Instance.audioSource.Play();
        }
    }

    public static void StopMusic(string SFXName)
    {
        if (Instance.sfxClips.ContainsKey(SFXName))
        {
            Instance.audioSource.clip = Instance.sfxClips[SFXName];
            Instance.audioSource.Stop();
        }
    }
    
}
