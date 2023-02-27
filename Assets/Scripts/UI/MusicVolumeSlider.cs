using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
    public static Button MuteButton;

    public static Slider Slider;

    public static bool IsMuted = false;

    void Awake()
    {
        MuteButton = GetComponentInChildren<Button>();
        Slider = GetComponentInChildren<Slider>();
        Slider.value = MusicManager.Instance.GetVolume();
    }

    public static void MuteVoice()
    {
        if (IsMuted)
        {
            MusicManager.Instance.UnMute();
            MuteButton.GetComponent<Image>().color = new Color(255, 255, 255);
            IsMuted = false;
        }
        else
        {
            MusicManager.Instance.Mute();
            MuteButton.GetComponent<Image>().color = new Color(255, 0, 0);
            IsMuted = true;
        }
    }

    public static void SetVoice()
    {
        if (IsMuted)
        {
            MuteVoice();
        }
        MusicManager.Instance.SetVolume(Slider.value);
    }
}
