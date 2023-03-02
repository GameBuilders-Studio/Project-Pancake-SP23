using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SFXVolumeSlider : MonoBehaviour
{
   public static Button muteButton;
   public static Slider slider;
   public static bool isMute= false;
   void Awake(){

      muteButton = GetComponentInChildren<Button>();
      slider = GetComponentInChildren<Slider>();
      slider.value = SFXManager.Instance.GetVolume();
   }
   public static void MuteVoice(){
      if(isMute){
         SFXManager.Instance.UnMute();
         muteButton.GetComponent<Image>().color= new Color(255, 255, 255);
         isMute = false;
      }
      else{
         SFXManager.Instance.Mute();
         muteButton.GetComponent<Image>().color= new Color(255, 0, 0);
         isMute = true;
      }
   }
   public static void SetVoice(){
      if(isMute){
         MuteVoice();
      }
      SFXManager.Instance.SetVolume(slider.value);

   }
}
