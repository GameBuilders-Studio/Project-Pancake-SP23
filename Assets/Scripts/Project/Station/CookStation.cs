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

    protected override void OnUpdate()
    {
        if (PlacedItem is CookContainer container)
        {
            Cook(container);
        }
    }

    void Cook(CookContainer container)
    {
        for (int i = 0; i < container.Capacity; i++)
        {
            var ingredient = container.Ingredients[i];

            if (ingredient.ProgressComplete) { continue; }

            ingredient.AddProgress(Time.deltaTime / _cookTimePerIngredient);

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
