using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookStation : Station
{
    [Space(15f)]
    [SerializeField]
    private float _cookTimePerIngredient;

    [SerializeField]
    private bool _cooking = false;

    private float _totalProgress = 0.0f;

    public float TotalProgress
    {
        get => _totalProgress;
        set => _totalProgress = value;
    }

    private CookContainer _container;

    protected override bool ValidateItem(Carryable item)
    {
        return item is CookContainer;
    }

    protected override void OnItemPlaced(Carryable item)
    {
        // limit type casting by caching CookContainer reference
        _container = item as CookContainer;
    }

    protected override void OnItemRemoved()
    {
        _container = null;
    }

    protected override void OnUpdate()
    {
        if (_container != null)
        {
            Cook(_container);
        }
    }

    void Cook(CookContainer container)
    {
        _totalProgress = 0.0f;

        for (int i = 0; i < container.Capacity; i++)
        {
            var ingredient = container.Ingredients[i];

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

    void OnCookComplete()
    {

    }
}
