using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingStation : Station<IngredientProp>
{
    [Space(15f)]
    [SerializeField]
    private float _totalChopTime;

    private IngredientProp _ingredientProp;

    public void Chop()
    {
        if (PlacedItemHasRequiredComponent(ref _ingredientProp))
        {
            _ingredientProp.AddProgress(Time.deltaTime / _totalChopTime);
        }
    }

    protected override void OnUpdate()
    {
        if (IsInteracting)
        {
            Chop();
        }
    }

    protected override bool ValidateItem(Selectable item)
    {
        return true;
    }
}
