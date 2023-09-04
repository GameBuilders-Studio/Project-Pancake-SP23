using Codice.Client.Common.GameUI;
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
    public const string SFX_VOLUME_KEY = "SfxVolume";

    private void Awake() {
        LoadVolume();
    }

    private void LoadVolume() {
        float masterVolume = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);

        _audioMixer.SetFloat(MASTER_VOLUME_KEY, Mathf.Log10(masterVolume) * 20);
        _audioMixer.SetFloat(MUSIC_VOLUME_KEY, Mathf.Log10(musicVolume) * 20);
        _audioMixer.SetFloat(SFX_VOLUME_KEY, Mathf.Log10(sfxVolume) * 20);
    }
}
