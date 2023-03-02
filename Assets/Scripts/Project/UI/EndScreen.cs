using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class EndScreen : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI EndText;
    [SerializeField] public ScoreUI scoreUI;

    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (EndText == null)
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

    private void OnDisable()
    {
        EventManager.RemoveListener("StartingLevel", OnStartLevel);
        EventManager.RemoveListener("Won", OnWon);
        EventManager.RemoveListener("Lost", OnLost);
    }

    private void OnStartLevel()
    {
        _canvasGroup.alpha = 0f;
    }
    private void OnWon()
    {
        EndText.text = string.Format("Time's Up! The score was {0}. You passed!", scoreUI.Score);
        _canvasGroup.alpha = 1f;
    }

    private void OnLost()
    {
        EndText.text = string.Format("Time's Up! Your score was {0}. You failed!", scoreUI.Score);
        _canvasGroup.alpha = 1f;
    }
}
