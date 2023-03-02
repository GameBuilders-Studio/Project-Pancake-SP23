using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class EndScreen : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _endText;
    [SerializeField] private ScoreUI _scoreUI;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_endText == null)
        {
            Debug.LogError("EndText is not assigned in EndScreen.cs");
        }
        if (_canvasGroup == null)
        {
            Debug.LogError("CanvasGroup is not assigned in EndScreen.cs");
        }
        _canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        EventManager.AddListener("StartingLevel", OnStartLevel);
        EventManager.AddListener("Won", OnWon);
        EventManager.AddListener("Lost", OnLost);
    }

    private void OnStartLevel()
    {
        _canvasGroup.alpha = 0f;
    }
    private void OnWon()
    {
        _endText.text = string.Format("Time's Up! The score was {0}. You passed!", _scoreUI.Score);
        _canvasGroup.alpha = 1f;
    }

    private void OnLost()
    {
        _endText.text = string.Format("Time's Up! Your score was {0}. You failed!", _scoreUI.Score);
        _canvasGroup.alpha = 1f;
    }
}
