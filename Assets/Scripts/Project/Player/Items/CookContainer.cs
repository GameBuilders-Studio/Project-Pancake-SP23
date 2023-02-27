using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookContainer : FoodContainer
{
    protected override bool ValidateIngredient(Ingredient ingredient)
    {
        return ingredient != null;
    }

    protected override void OnAddIngredient()
    {
        // do something
    }
}
