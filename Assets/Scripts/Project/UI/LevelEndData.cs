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
    private TextMeshProUGUI _baseScoreText; 

    [SerializeField]
    private TextMeshProUGUI _bonusScoreText; 

    [SerializeField]
    private TextMeshProUGUI _scoreDeductionText; 

    [SerializeField]
    private TextMeshProUGUI _totalScoreText;

    [SerializeField]
    private StarBar _starBar;

    private void Start()
    {
        _levelNameText.text = DataCapsule.Instance.lastLevel;
        _baseScoreText.text = DataCapsule.Instance.baseScore.ToString();
        _bonusScoreText.text = DataCapsule.Instance.bonusScore.ToString();
        _scoreDeductionText.text = "-" + DataCapsule.Instance.scoreDeduction.ToString();
        _totalScoreText.text = DataCapsule.Instance.totalScore.ToString();
        _starBar.SetTargetFraction((float) DataCapsule.Instance.totalScore / DataCapsule.Instance.scoreBarMax);
        _starBar.PlayStarAnimation();
    }
}
