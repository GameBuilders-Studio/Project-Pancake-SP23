using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CustomAttributes;

[RequireComponent(typeof(CanvasGroup))]
public class ScoreUI : MonoBehaviour
{
    [SerializeField, Required] ProgressBar _comboBar;

    [Tooltip("Score text to display the score")]
    [SerializeField, Required] public TextMeshProUGUI ScoreText;

    [Tooltip("Score to win the game")]
    [SerializeField] public int ScoreToWin;

    [SerializeField, Required] private GameObject _endScreen;
    private int _score;

    private CanvasGroup _canvasGroup;

    public bool GetIsWin() => _score >= ScoreToWin;

    public int Score {
        get => _score;
        set => _score = value;
    }

    private void Awake()
    {
        _score = 0;
        UpdateScoreText();
        _canvasGroup = GetComponent<CanvasGroup>();
        if (ScoreText == null)
        {
            Debug.LogError("ScoreText is not assigned in ScoreUI.cs");
        }
        if (_canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is not assigned in ScoreUI.cs");
        }
        _canvasGroup.alpha = 0f;
    }
    private void OnEnable()
    {
        EventManager.AddListener("StartingLevel", OnStartLevel);

        // suddenly you don't need those anymore
        // EventManager.AddListener("IncrementingScore", OnScoreIncremented);
        // EventManager.AddListener("DecrementingScore", OnScoreDecremented);
        //EventManager.AddListener("TimerEnded", OnTimerEnded);
    }

    private void OnStartLevel()
    {
        _score = 0;
        UpdateScoreText();
        _canvasGroup.alpha = 1f;
        _endScreen.SetActive(false);
    }

    // /// <summary>
    // /// Increment score by 1 and update the score text after IncrementingScore event.
    // /// </summary>
    // private void OnScoreIncremented()
    // {
    //     _score++;
    //     UpdateScoreText();
    // }
    //
    // /// <summary>
    // /// Decrement score by 1 and update the score text after IncrementingScore event.
    // /// </summary>
    // private void OnScoreDecremented()
    // {
    //     if (_score <= 0) return;
    //     _score--;
    //     UpdateScoreText();
    // }

    // private void OnTimerEnded()
    // {
    //     _endScreen.SetActive(true);
    //     if (GetIsWin())
    //     {
    //         EventManager.Invoke("Won");
    //     }
    //     else { EventManager.Invoke("Lost"); }
    //
    //     _canvasGroup.alpha = 0f;
    // }

    private void Update()
    {
        // In the future we might want to change this
        // to only call when something is triggered
        // for now this is fine - Revan
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        ScoreText.text = $"Score: {_score}";
    }

}
