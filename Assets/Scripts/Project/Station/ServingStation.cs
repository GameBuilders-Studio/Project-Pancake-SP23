using UnityEngine;
using System.Collections.Generic;

public class ServingStation : StationBehaviour
{
    [SerializeField] private OrderSystem _orderSystem;
    public override void ItemPlaced(ref Carryable item)
    {
        Debug.Log("Called OnItemPlaced");
        Destroy(item.gameObject);
    }

    public override bool ValidateItem(Carryable item)
    {
        //Only Accept dish, don't accept ingredient
        if (item is FoodContainer container)
        {
            List<IngredientType> ingredientTypes = new();
            foreach (var ingredient in container.Ingredients)
            {
                ingredientTypes.Add(ingredient.Type);
            }
            _orderSystem.CheckOrderMatch(ingredientTypes);
            container.ClearIngredients();
            Destroy(container.gameObject);
            return true;
        }
        return false;
    }

    public bool isOrderCorrect(FoodContainer container)
    {
        return true;
    }
}
