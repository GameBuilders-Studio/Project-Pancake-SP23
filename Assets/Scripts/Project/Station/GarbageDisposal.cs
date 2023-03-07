using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageDisposal : Station
{
    protected override void OnItemPlaced(Carryable item)
    {
        Destroy(item.gameObject);
    }

    public override bool TryPlaceItem(Carryable item)
    {
        if (item is FoodContainer container)
        {
            container.ClearIngredients();
            return false;
        }

        if (item.TryGetComponent(out IngredientProp ingredientProp))
        {
            Destroy(ingredientProp.gameObject);
            return true;
        }

        return false;
    }
}
