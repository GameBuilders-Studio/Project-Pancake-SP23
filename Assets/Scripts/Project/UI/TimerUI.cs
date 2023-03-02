using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class TimerUI : MonoBehaviour
{
    [Tooltip("Time text to display the time")]
    [SerializeField] public TextMeshProUGUI TimeText;

    [Tooltip("Time Duration in seconds")]
    [SerializeField] public float TimeDuration;
    private float _timeRemaining = 0;
    private bool _timerIsRunning;
    private bool _timerUp; //Bool to check if timer has hit zero already

    private CanvasGroup _canvasGroup;

    public void OnStartLevel()
    {
        _timeRemaining = TimeDuration;
        _timerIsRunning = true;
        _timerUp = false;
        _canvasGroup.alpha = 1f;
    }

    public void OnResumeTimer() => _timerIsRunning = true;
    public void OnStopTimer() => _timerIsRunning = false;



    private void Awake()
    {
        _timerIsRunning = false;
        _timerUp = false;
        _canvasGroup = GetComponent<CanvasGroup>();


        if (TimeText == null)
        {
            Debug.LogError("TimeText is not assigned in TimerUI.cs");
        }
        if (_canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is not assigned in TimerUI.cs");
        }

        _canvasGroup.alpha = 0f;

    }

    private void OnEnable()
    {
        EventManager.AddListener("StartingLevel", OnStartLevel);
        EventManager.AddListener("ResumingTimer", OnResumeTimer);
        EventManager.AddListener("StoppingTimer", OnStopTimer);
    }

    void Update()
    {
        if (_timerIsRunning)
        {
            _timeRemaining = Mathf.Max(_timeRemaining - Time.deltaTime, 0);
            SetTimeText(_timeRemaining);
            if (_timeRemaining == 0 && !_timerUp)
            {
                _timerUp = true;
                _canvasGroup.alpha = 0f;
                EventManager.Invoke("TimerEnded");
            }
        }
    }

    private void SetTimeText(float time)
    {
        float minutes = Mathf.FloorToInt(time / 60);
        float seconds = Mathf.FloorToInt(time % 60);
        TimeText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
    }
}
