using UnityEngine;
using System.Collections;

public class Stove : StationController
{
    [SerializeField]
    private float _cookTimePerIngredient;

    [SerializeField]
    private float _overcookTime;

    [SerializeField]
    private IngredientStateData _targetIngredientState;

    [SerializeField]
    private IngredientStateData _targetOvercookedIngredientState;

    [SerializeField]
    private bool _cooking = false;

    private float _overcookTimeRemaining;
    private Coroutine _timerCoroutine;
    private float _totalProgress = 0.0f;
    private Pot _container;
    private bool _containerExists = false;

    public float TotalProgress => _totalProgress;

    void Update()
    {
        if (!_containerExists) { return; }
        _cooking = true;
        Cook(_container);
    }

    public override bool ValidateItem(Carryable item)
    {
        return true;
    }

    public override void ItemPlaced(ref Carryable item)
    {
        // limit type casting by caching Pot reference
        _containerExists = item.TryGetBehaviour(out _container);
    }

    public override void ItemRemoved(ref Carryable item)
    {
        _container = null;
        _containerExists = false;
        _cooking = false;
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
        }
    }

    // we need to cook jesse
    void Cook(Pot container)
    {
        _totalProgress = 0.0f;

        for (int i = 0; i < container.Count; i++)
        {
            var ingredient = container.Ingredients[i];

            if (ingredient.State != _targetIngredientState && ingredient.State != _targetOvercookedIngredientState) //only transition states if not already cooking or overcooked
            {
                ingredient.SetState(_targetIngredientState);
            }

            if (i == container.Count - 1 && ingredient.ProgressComplete && _timerCoroutine == null) // if last ingredient is done cooking, start timer
            {
                if (ingredient.State != _targetOvercookedIngredientState) // don't call timer if already overcooked
                {
                    OnCookComplete(container);
                }
            }

            if (ingredient.ProgressComplete)
            {
                _totalProgress += 1.0f / container.Count;
                continue;
            }

            ingredient.AddProgress(Time.deltaTime / _cookTimePerIngredient);
            _totalProgress += ingredient.Progress / container.Count;

            return;
        }
    }

    void OnCookComplete(Pot container)
    {
        // Debug.Log("Cook Complete");
        _overcookTimeRemaining = _overcookTime;
        _timerCoroutine = StartCoroutine(TimerCoroutine(container));
    }

    private IEnumerator TimerCoroutine(Pot container)
    {
        while (_overcookTimeRemaining > 0)
        {
            Debug.Log(_overcookTimeRemaining);
            _overcookTimeRemaining -= Time.deltaTime;
            yield return null;
        }
        // Debug.Log("Overcooked!");
        for (int i = 0; i < container.Count; i++)
        {
            var ingredient = container.Ingredients[i];
            ingredient.SetState(_targetOvercookedIngredientState);
            ingredient.SetProgress(1.0f);
        }

    }
}
