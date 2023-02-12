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
    [SerializeField] public GameObject WinText; //Dunno y here
    private int _score;
    private void Awake()
    {
        _score = 0;
        UpdateScoreText();
        WinText.SetActive(false); //Dunno y here
    }
    private void OnEnable()
    {
        EventManager.AddListener("OnIncrementScore", HandleIncrementScore);
        EventManager.AddListener("OnDecrementScore", HandleDecrementScore);
        EventManager.AddListener("OnTimerEnd", HandleTimerEnd);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("OnIncrementScore", HandleIncrementScore);
        EventManager.RemoveListener("OnDecrementScore", HandleDecrementScore);
        EventManager.AddListener("OnTimerEnd", HandleTimerEnd);
    }

    /// <summary>
    /// Increment score by 1 and update the score text after OnIncrementScore event.
    /// </summary>
    private void HandleIncrementScore()
    {
        _score++;
        UpdateScoreText();
    }

    /// <summary>
    /// Decrement score by 1 and update the score text after OnIncrementScore event.
    /// </summary>
    private void HandleDecrementScore()
    {
        if (_score <= 0) return;
        _score--;
        UpdateScoreText();
    }

    private void HandleTimerEnd()
    {
        if (GetIsWin())
        {
            EventManager.Invoke("OnWin");
            WinText.SetActive(true); //Dunno y here

        }
        else EventManager.Invoke("OnLose");
    }

    private void UpdateScoreText()
    {
        ScoreText.text = "Score: " + _score.ToString();

    }

    public bool GetIsWin() => _score >= ScoreToWin;
}
