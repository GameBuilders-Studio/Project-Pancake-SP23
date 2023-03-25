using UnityEngine;
using System.Collections.Generic;
using CustomAttributes;
public class ServingStation : StationController
{
    [SerializeField, Required] private OrderSystem _orderSystem;
    public override void ItemPlaced(ref Carryable item)
    {
        Debug.Log("Called OnItemPlaced");
        Destroy(item.gameObject);
    }

    public override bool ValidateItem(Carryable item)
    {
        //Only Accept dish, don't accept ingredient
        if (item.TryGetBehaviour(out FoodContainer container))
        {
            isOrderCorrect(container);
            container.ClearIngredients();
            Destroy(container.gameObject);
            return true;
        }
        return false;
    }

    public bool isOrderCorrect(FoodContainer container)
    {
        List<IngredientType> ingredientTypes = new();
        foreach (var ingredient in container.Ingredients)
        {
            ingredientTypes.Add(ingredient.Type);
        }
        return _orderSystem.CheckOrderMatch(ingredientTypes);
    }
}
