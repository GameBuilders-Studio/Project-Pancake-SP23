using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// This class handles the data displayed on the level end screen
public class LevelEndData : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _levelNameText;

    [SerializeField]
    private TextMeshProUGUI _scoreText; 

    private void Start()
    {
        _levelNameText.text = DataCapsule.Instance.lastLevel;
        _scoreText.text = DataCapsule.Instance.score.ToString();
    }
}
