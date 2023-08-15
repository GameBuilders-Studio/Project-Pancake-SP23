using System.Collections.Generic;
using CustomAttributes;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField, Required]
    private AudioMixer _audioMixer;

    [SerializeField, Required]
    private Slider _masterVolumeSlider;

    [SerializeField, Required]
    private Slider _musicVolumeSlider;

    [SerializeField, Required]
    private Slider _sfxVolumeSlider;

    public const string MASTER_VOLUME_KEY = "MasterVolume";
    public const string MUSIC_VOLUME_KEY = "MusicVolume";
    public const string SFX_VOLUME_KEY = "SfxVolume";

    private void Awake() {
        _masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
        _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
    }

    private void Start() {
        // Set the sliders to the saved values
        _masterVolumeSlider.value = PlayerPrefs.GetFloat(MASTER_VOLUME_KEY, 1f);
        _musicVolumeSlider.value = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        _sfxVolumeSlider.value = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f);
    }

    private void OnMasterVolumeChanged(float value) {
        _audioMixer.SetFloat(MASTER_VOLUME_KEY, Mathf.Log10(value) * 20);

        // Save the value to PlayerPrefs so it can be saved between scenes
        PlayerPrefs.SetFloat(MASTER_VOLUME_KEY, value);
    }

    private void OnMusicVolumeChanged(float value) {
        _audioMixer.SetFloat(MUSIC_VOLUME_KEY, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
    }

    private void OnSfxVolumeChanged(float value) {
        _audioMixer.SetFloat(SFX_VOLUME_KEY, Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
    }
}
