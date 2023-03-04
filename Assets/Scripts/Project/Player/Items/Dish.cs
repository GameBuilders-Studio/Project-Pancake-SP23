using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dish : FoodContainer
{
    protected override bool ValidateIngredient(Ingredient ingredient)
    {
        return ingredient.ProgressComplete;
    }
}
