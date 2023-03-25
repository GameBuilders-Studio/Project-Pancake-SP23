using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CustomAttributes;

public class Order : MonoBehaviour
{
    [SerializeField] private RecipeData _recipe;

    [SerializeField, Required]
    private TextMeshProUGUI _orderText;

    [SerializeField] private OrderProgressBar _orderProgressBar;

    public bool IsComplete { get; set; }
    public float TimeRemaining { get; private set; }

    //todo: variable for complete time to use for points?
    [SerializeField] private float _startTime = 15f;
    public RecipeData RecipeData { get => _recipe; }

    private Coroutine _timerCoroutine;

    public void SetOrderComplete()
    {
        IsComplete = true;
        StopTimer();
    }

    private void Awake()
    {
        SetTimer(_startTime);
        StartTimer();
    }

    private void OnDisable()
    {
        StopTimer();
    }

    private void SetTimer(float seconds)
    {
        TimeRemaining = seconds;
    }

    private void StartTimer()
    {
        UpdateOrderText();
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
        SetTimer(_startTime);
        StartTimer();
    }

    private IEnumerator TimerCoroutine()
    {
        while (TimeRemaining > 0)
        {
            TimeRemaining -= Time.deltaTime;

            _orderProgressBar.SetFill(TimeRemaining / _startTime);
            yield return null;
        }
        EventManager.Invoke("OrderExpired");
        // Debug.Log("Order Expired");
    }

    private void UpdateOrderText()
    {
        _orderText.text = $"Recipe Name: {_recipe.RecipeName}";
    }

}
