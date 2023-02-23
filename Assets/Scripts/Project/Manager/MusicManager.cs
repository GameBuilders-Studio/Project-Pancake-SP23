using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class MusicDictionary : SerializableDictionary<string, AudioClip>
{ }
[RequireComponent(typeof(AudioSource))]
public class MusicManager : Singleton<MusicManager>
{
    public MusicDictionary musicClips = new MusicDictionary();
    private AudioSource audioSource1;
    private AudioSource audioSource2;
    private float volume = 0.5f; // TODO: Only for testing purpose
    private static bool haveDone = false;
    private string playName;
    [SerializeField]
    private float fadeTime = 2f;
    void Awake()
    {
        audioSource1 = GetComponents<AudioSource>()[0];
        audioSource2 = GetComponents<AudioSource>()[1];
    }

    void Start()
    {
        if (haveDone)
        {
            return;
        }
        Instance.PlayMusic("Menu"); ;
        EventManager.AddListener("StartMenu", new UnityAction(() => Instance.PlayMusic("Menu"))); //When you enter into the menu
        EventManager.AddListener("StartTutorial", new UnityAction(() => Instance.PlayMusic("Round"))); //When you press the start button
        EventManager.AddListener("RoundStart", new UnityAction(() => Instance.PlayMusic("Round"))); //When you press the start button
        EventManager.AddListener("GameLost", new UnityAction(() => Instance.PlayMusic("GameLost"))); //When you press the start button
        EventManager.AddListener("GameWon", new UnityAction(() => Instance.PlayMusic("GameWon"))); //When you press the start button
        haveDone = true;
    }
    public void SetVolume(float num)
    {
        volume = num;
        audioSource1.volume = volume;
        audioSource2.volume = volume;
    }
    public void Mute()
    {
        audioSource1.mute = true;
        audioSource2.mute = true;
    }
    public void UnMute()
    {
        audioSource1.mute = false;
    }
    public float GetVolume()
    {
        return volume;
    }

    public void PlayMusic(string musicName)
    {
        if (musicClips.ContainsKey(musicName))
        {
            if (playName == musicName)
                //if the music is playing, don't play it again
                return;
            playName = musicName;
            PlayFadeMusic(musicClips[musicName], audioSource1, audioSource2);
        }
        else
        {
            Debug.LogWarning("MusicManager: No music named " + musicName);
        }
    }

    public void StopMusic()
    {
        audioSource1.Stop();
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
        float endTime = startTime + fadeTime;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / fadeTime;
            audioSource.volume = Mathf.Lerp(volume, 0.0f, t);
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
        float endTime = startTime + fadeTime;
        while (Time.time < endTime)
        {
            float t = (Time.time - startTime) / fadeTime;
            audioSource.volume = Mathf.Lerp(0.0f, volume, t);
            yield return null;
        }
        audioSource.volume = volume;
    }
}
