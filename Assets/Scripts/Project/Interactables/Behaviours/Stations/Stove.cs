using UnityEngine;

public class Stove : StationController
{
    [SerializeField]
    private float _cookTimePerIngredient;

    [SerializeField]
    private IngredientStateData _targetIngredientState;

    [SerializeField]
    private bool _cooking = false;

    private float _totalProgress = 0.0f;
    private Pot _container;
    private bool _containerExists = false;

    public float TotalProgress => _totalProgress;

    void Update()
    {
        if (!_containerExists) { return; }
        Cook(_container);
    }

    public override bool ValidateItem(Carryable item)
    {
        return item.HasBehaviour<Pot>();
    }

    public override void ItemPlaced(ref Carryable item)
    {
        _containerExists = item.TryGetBehaviour(out _container);
    }

    public override void ItemRemoved(ref Carryable item)
    {
        _container = null;
        _containerExists = false;
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
