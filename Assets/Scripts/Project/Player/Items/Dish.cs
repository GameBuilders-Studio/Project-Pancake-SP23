using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dish : FoodContainer
{
    protected override bool ValidateIngredient(Ingredient ingredient)
    {
        return false;
    }

    protected override bool ValidateTransfer(FoodContainer other)
    {
        if (!other.IsFull) { return false; }

        for (int i = 0; i < other.Count; i++)
        {
            var ingredient = other.Ingredients[i];
            if (!ingredient.ProgressComplete) { return false; }
        }

        return true;
    }
}
