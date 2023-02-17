using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreUI : MonoBehaviour
{
    [Tooltip("Score text to display the score")]
    [SerializeField] public TextMeshProUGUI ScoreText;

    [Tooltip("Score to win the game")]
    [SerializeField] public int ScoreToWin;
    [SerializeField] public GameObject WinText;
    private int _score;

    public bool GetIsWin() => _score >= ScoreToWin;
    private void Awake()
    {
        _score = 0;
        UpdateScoreText();
        WinText.SetActive(false);
    }
    private void OnEnable()
    {
        EventManager.AddListener("IncrementingScore", OnScoreIncremented);
        EventManager.AddListener("DecrementingScore", OnScoreDecremented);
        EventManager.AddListener("TimerEnded", OnTimerEnded);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("IncrementingScore", OnScoreIncremented);
        EventManager.RemoveListener("DecrementingScore", OnScoreDecremented);
        EventManager.AddListener("TimerEnded", OnTimerEnded);
    }

    /// <summary>
    /// Increment score by 1 and update the score text after IncrementingScore event.
    /// </summary>
    private void OnScoreIncremented()
    {
        _score++;
        UpdateScoreText();
    }

    /// <summary>
    /// Decrement score by 1 and update the score text after IncrementingScore event.
    /// </summary>
    private void OnScoreDecremented()
    {
        if (_score <= 0) return;
        _score--;
        UpdateScoreText();
    }

    private void OnTimerEnded()
    {
        if (GetIsWin())
        {
            EventManager.Invoke("Won");
            WinText.SetActive(true);
        }
        else EventManager.Invoke("Lost");
    }

    private void UpdateScoreText()
    {
        ScoreText.text = $"Score: {_score}";

    }


}
