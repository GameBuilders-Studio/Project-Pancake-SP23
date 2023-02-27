using System.Collections;
using UnityEngine;

[System.Serializable]
public class MusicDictionary : SerializableDictionary<string, AudioClip> { }

[RequireComponent(typeof(AudioSource))]
public class MusicManager : Singleton<MusicManager>
{
    [SerializeField]
    private float _fadeTime = 2f;

    [SerializeField]
    public MusicDictionary musicClips = new();

    private AudioSource _audioSource1;
    private AudioSource _audioSource2;

    private float _volume = 0.5f; // TODO: Only for testing purpose
    private string _playName;

    void Awake()
    {
        var audioSources = GetComponents<AudioSource>();
        _audioSource1 = audioSources[0];
        _audioSource2 = audioSources[1];
    }

    public void SetVolume(float num)
    {
        _volume = num;
        _audioSource1.volume = _volume;
        _audioSource2.volume = _volume;
    }

    public void Mute()
    {
        _audioSource1.mute = true;
        _audioSource2.mute = true;
    }

    public void UnMute()
    {
        _audioSource1.mute = false;
    }

    public float GetVolume()
    {
        return _volume;
    }

    public void PlayMusic(string musicName)
    {
        if (musicClips.ContainsKey(musicName))
        {
            if (_playName == musicName) { return; }
            _playName = musicName;
            PlayFadeMusic(musicClips[musicName], _audioSource1, _audioSource2);
        }
        else
        {
            Debug.LogWarning("MusicManager: No music named " + musicName);
        }
    }

    public void StopMusic()
    {
        _audioSource1.Stop();
    }

    public void PlayFadeMusic(AudioClip newClip, AudioSource audioSource1, AudioSource audioSource2)
    {
        // Check if the first audio source is playing
        Debug.Log("audioSource1:" + audioSource1.isPlaying);
        Debug.Log("audioSource2:" + audioSource2.isPlaying);
        if (audioSource1.isPlaying)
        {
            // Start fading out the first audio source
            StartCoroutine(FadeOut(audioSource1));
            // Start fading in the second audio source
            StartCoroutine(FadeIn(audioSource2, newClip));
        }
        else
        {
            // Start fading out the second audio source
            StartCoroutine(FadeOut(audioSource2));
            // Start fading in the first audio source
            StartCoroutine(FadeIn(audioSource1, newClip));
        }
    }

    private IEnumerator FadeOut(AudioSource audioSource)
    {
        float startTime = Time.time;
        float endTime = startTime + _fadeTime;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / _fadeTime;
            audioSource.volume = Mathf.Lerp(_volume, 0.0f, t);
            yield return null;
        }
        audioSource.Stop();
        audioSource.volume = 0;
    }

    private IEnumerator FadeIn(AudioSource audioSource, AudioClip newClip)
    {
        audioSource.clip = newClip;
        audioSource.volume = 0;
        audioSource.Play();
        float startTime = Time.time;
        float endTime = startTime + _fadeTime;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / _fadeTime;
            audioSource.volume = Mathf.Lerp(0.0f, _volume, t);
            yield return null;
        }
        audioSource.volume = _volume;
    }
}
