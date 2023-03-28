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
        List<IngredientData> ingredientDatas = new();
        foreach (var ingredient in container.Ingredients)
        {
            ingredientDatas.Add(ingredient.Data);
        }
        return _orderSystem.CheckOrderMatch(ingredientDatas);
    }
}
