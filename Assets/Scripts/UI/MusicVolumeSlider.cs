using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MusicVolumeSlider : MonoBehaviour
{
   public static Button muteButton;
   public static Slider slider;
   public static bool isMute= false;
   void Awake(){

      muteButton = GetComponentInChildren<Button>();
      slider = GetComponentInChildren<Slider>();
      slider.value = MusicManager.Instance.GetVolume();
   }
   public static void MuteVoice(){
      if(isMute){
         MusicManager.Instance.UnMute();
         muteButton.GetComponent<Image>().color= new Color(255, 255, 255);
         isMute = false;
      }
      else{
         MusicManager.Instance.Mute();
         muteButton.GetComponent<Image>().color= new Color(255, 0, 0);
         isMute = true;
      }
   }
   public static void SetVoice(){
      if(isMute){
         MuteVoice();
      }
      MusicManager.Instance.SetVolume(slider.value);

   }
}
