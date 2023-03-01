using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerUI : MonoBehaviour
{
    [Tooltip("Time text to display the time")]
    [SerializeField] public TextMeshProUGUI TimeText;

    [Tooltip("Time Duration in seconds")]

    private float _timeRemaining = 0;
    private bool _timerIsRunning;
    private bool _timerUp; //Bool to check if timer has hit zero already
    void Update()
    {
        if (_timerIsRunning)
        {
            _timeRemaining = Mathf.Max(_timeRemaining - Time.deltaTime, 0);
            SetTimeText(_timeRemaining);
            if (_timeRemaining == 0 && !_timerUp)
            {
                _timerUp = true;
                EventManager.Invoke("TimerEnded");
            }
        }
    }
    public void StartTimer(float startTime)
    {
        _timeRemaining = startTime;
        _timerIsRunning = true;
        _timerUp = false;
    }

    public void ResumeTimer() => _timerIsRunning = true;
    public void StopTimer() => _timerIsRunning = false;


    private void SetTimeText(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        TimeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
