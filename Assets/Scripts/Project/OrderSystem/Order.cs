using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CustomAttributes;
[RequireComponent(typeof(RectTransform))]
public class Order : MonoBehaviour
{
    [SerializeField] private RecipeData _recipe;

    [SerializeField] private float _startTime = 15f;

    [SerializeField] private float _orderDespawnTime = 10f;

    [SerializeField, Required] private ProgressBar _orderProgressBar;

    [SerializeField, Required] private Image _panel;


    public bool IsComplete { get; set; }
    public float TimeRemaining { get; private set; }


    // variable for complete time to use for points?
    public RecipeData RecipeData { get => _recipe; }
    public RectTransform RectTransform { get => _rectTransform; }

    private RectTransform _rectTransform;
    private Coroutine _timerCoroutine;
    private float _orderDespawnTimeRemaining;
    private Coroutine _despawnTimerCoroutine;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _orderProgressBar.SetMaxValue(_startTime);
        SetTimer(_startTime);
        StartTimer();
    }

    private void OnDisable()
    {
        StopTimer();
    }

    public void SetOrderComplete()
    {
        IsComplete = true;
        StopTimer();
        _panel.color = Color.green;
    }

    public void DespawnOrder()
    {
        if (_despawnTimerCoroutine != null)
        {
            return;
        }

        StopAllCoroutines();
        _orderDespawnTimeRemaining = _orderDespawnTime;
        _despawnTimerCoroutine = StartCoroutine(DespawnTimerCoroutine());
    }

    public void SetStartTime(float time)
    {
        _startTime = time;
        SetTimer(_startTime);
        _orderProgressBar.SetMaxValue(_startTime);
    }

    public void StartTimer()
    {
        _timerCoroutine = StartCoroutine(TimerCoroutine());
    }

    public void StopTimer()
    {
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }

    }

    public void ResumeTimer()
    {
        if (_timerCoroutine == null)
        {
            _timerCoroutine = StartCoroutine(TimerCoroutine());
        }
    }

    public void ResetTimer()
    {
        StopTimer();
        SetTimer(_startTime);
        StartTimer();
    }

    private void SetTimer(float seconds)
    {
        TimeRemaining = seconds;
    }

    private IEnumerator TimerCoroutine()
    {
        while (TimeRemaining > 0)
        {
            _orderProgressBar.SetProgress(TimeRemaining);
            TimeRemaining -= Time.deltaTime;
            yield return null;
        }
        EventManager.Invoke("OrderExpired");
    }

    private IEnumerator DespawnTimerCoroutine()
    {
        while (_orderDespawnTimeRemaining > 0)
        {
            _orderDespawnTimeRemaining -= Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }

}
