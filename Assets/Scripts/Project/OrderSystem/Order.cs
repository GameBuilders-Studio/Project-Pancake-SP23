using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Order : MonoBehaviour
{
    [SerializeField]
    private RecipeData _recipe;

    [SerializeField]
    private TextMeshProUGUI _orderText;
    public bool IsComplete { get; set; }
    public float TimeRemaining { get; set; }

    //todo: variable for complete time to use for points?
    [SerializeField]
    private float _defaultTimeRemaining = 15f;
    public RecipeData RecipeData => _recipe;

    private Coroutine _timerCoroutine;

    //Testing
    private void Awake()
    {
        SetTimer(_defaultTimeRemaining);
        StartTimer();
    }

    private void SetTimer(float seconds)
    {
        TimeRemaining = seconds;
    }

    private void StartTimer()
    {
        _timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    private void StopTimer()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(TimerCoroutine());
            _timerCoroutine = null;
        }

    }

    private void ResumeTimer()
    {
        if (_timerCoroutine == null)
        {
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }
    }

    private void ResetTimer()
    {
        StopTimer();
        SetTimer(_defaultTimeRemaining);
        StartTimer();
    }

    private IEnumerator TimerCoroutine()
    {
        while (TimeRemaining > 0)
        {
            TimeRemaining -= Time.deltaTime;
            UpdateOrderText();
            yield return null;
        }
        EventManager.Invoke("OrderExpired");
        Debug.Log("Order Expired");
    }

    public void SetOrderComplete()
    {
        IsComplete = true;
        StopTimer();
    }

    private void UpdateOrderText()
    {
        float minutes = Mathf.FloorToInt(TimeRemaining / 60);
        float seconds = Mathf.FloorToInt(TimeRemaining % 60);
        string time = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        _orderText.text = $"Recipe Name: {_recipe.RecipeName} \n Time Remaining: {time}";
    }

}
