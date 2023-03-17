using UnityEngine;

public class CookStation : StationBehaviour
{
    [SerializeField]
    private float _cookTimePerIngredient;

    [SerializeField]
    private IngredientStateData _targetIngredientState;

    [SerializeField]
    private bool _cooking = false;

    private float _totalProgress = 0.0f;
    private CookContainer _container;
    private bool _containerExists = false;

    public float TotalProgress => _totalProgress;

    void Update()
    {
        if (!_containerExists) { return; }
        Cook(_container);
    }

    public override bool ValidateItem(Carryable item)
    {
        return item is CookContainer;
    }

    public override void ItemPlaced(ref Carryable item)
    {
        // limit type casting by caching CookContainer reference
        _container = item as CookContainer;
        _containerExists = _container != null;
    }

    public override void ItemRemoved(ref Carryable item)
    {
        _container = null;
        _containerExists = false;
    }

    // we need to cook jesse
    void Cook(CookContainer container)
    {
        _totalProgress = 0.0f;

        for (int i = 0; i < container.Count; i++)
        {
            var ingredient = container.Ingredients[i];

            ingredient.SetState(_targetIngredientState);

            if (ingredient.ProgressComplete) 
            {
                _totalProgress += 1.0f / container.Count;
                continue; 
            }

            ingredient.AddProgress(Time.deltaTime / _cookTimePerIngredient);
            _totalProgress += ingredient.Progress / container.Count;

            _cooking = true;

            if (i == container.Capacity - 1 && ingredient.ProgressComplete)
            {
                if (_cooking)
                {
                    _cooking = false;
                    OnCookComplete();
                }
            }

            return;
        }
    }

    void OnCookComplete() {}
}
