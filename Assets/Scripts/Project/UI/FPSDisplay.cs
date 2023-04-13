using UnityEngine;
using TMPro;

public class NewBehaviourScript : MonoBehaviour
{
    //Declaring variables
    public TextMeshProUGUI FPSText;

    private float pollingTime = 1f;
    private float time; 
    private int frameCount;


    void Update()
    {
        time += Time.deltaTime;

        frameCount++;

        if(time >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / time);
            FPSText.text = frameRate.ToString() + " FPS";

            time -= pollingTime;
            frameCount = 0;
        }
    }
}
