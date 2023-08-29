using UnityEngine;
using System.Collections;
using CustomAttributes;
using UnityEngine.EventSystems;

public class Stove : StationController
{
    [SerializeField]
    private float _cookTimePerIngredient;

    [SerializeField]
    private float _overcookTime;

    [SerializeField] private float _flashFinishTime = 4f;

    [SerializeField, Required]
    private IngredientStateData _targetIngredientState;

    [SerializeField, Required]
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
        // If there is nothing, only pot can be placed
        if(!_containerExists) { 
            return item.HasBehaviour<Pot>();
        } else {
            if (item.TryGetBehaviour(out IngredientProp ingredientProp))
            {
                return _container.ValidateIngredient(ingredientProp.Ingredient);
            }
            return false; 
        }
    }

    public override void ItemPlaced(ref Carryable item) 
    {
        _containerExists = item.TryGetBehaviour(out _container);
        if (_containerExists)
        {
            _container.StartFlashing(_overcookTimeRemaining);
        }
    }

    public override void ItemRemoved(ref Carryable item)
    {
        _containerExists = false;
        _cooking = false;
        _container.DisableCookingVisual();
        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
            _timerCoroutine = null;
            if (_container != null)
            {
                _container.StopFlashing();
            }
        }
        _container = null;
    }

    public void StartFire()
    {
        if (TryGetBehaviour(out Flammable flammable))
        {
            flammable.TryIgnite();
        }
    }

    // we need to cook jesse
    void Cook(Pot container)
    {
        if(container.IsEmpty) { return; }

        _container.EnableCookingVisual();
        _totalProgress = 0.0f;

        for (int i = 0; i < container.Count; i++)
        {
            var ingredient = container.Ingredients[i];

            if (ingredient.State != container.targetCookState && ingredient.State != _targetOvercookedIngredientState) //only transition states if not already cooking or overcooked
            {
                container.SetIngredientState(ingredient, container.targetCookState);
            }

            // if last ingredient is done cooking, start overcook (1f-2f)
            if (i == container.Count - 1 && ingredient.ProgressComplete && _timerCoroutine == null)
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
            container.SetProgress(_totalProgress);

            return;
        }
    }

    void OnCookComplete(Pot container)
    {
        _overcookTimeRemaining = _overcookTime;
        _timerCoroutine = StartCoroutine(OverCookCoroutine(container));
    }

    private IEnumerator OverCookCoroutine(Pot container)
    {
        container.StartFlashing(_overcookTimeRemaining);
        while (_overcookTimeRemaining > 0)
        {
            _overcookTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < container.Count; i++)
        {
            var ingredient = container.Ingredients[i];
            ingredient.SetState(_targetOvercookedIngredientState);
            ingredient.SetProgress(1.0f);
        }

        container.Overcooked();
    }

}
