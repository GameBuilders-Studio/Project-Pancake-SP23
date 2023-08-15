using System.Collections;
using System.Collections.Generic;
using CustomAttributes;
using GameBuilders.Singleton;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : Singleton<AudioManager>
{
    [SerializeField, Required]
    private AudioMixer _audioMixer;
    // Load in audio settings

    public const string MASTER_VOLUME_KEY = "MasterVolume";
    public const string MUSIC_VOLUME_KEY = "MusicVolume";
    public const string SFX_VOLUME_KEY = "SFXVolume";

    private void Awake() {
        // Load in audio settings
        _audioMixer.SetFloat(MASTER_VOLUME_KEY, PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 0));
        _audioMixer.SetFloat(MUSIC_VOLUME_KEY, PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 0));
        _audioMixer.SetFloat(SFX_VOLUME_KEY, PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 0));
    }
}
