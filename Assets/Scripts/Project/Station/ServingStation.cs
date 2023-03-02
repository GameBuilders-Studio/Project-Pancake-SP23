using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServingStation : Station
{
    protected override void OnItemPlaced(Carryable item)
    {
        Debug.Log("Called OnItemPlaced");
        Destroy(item.gameObject);
    }

    public override bool TryPlaceItem(Carryable item)
    {
        if(OrderMatch()) {
            EventManager.Invoke("IncrementingScore");
        }
        if (item is FoodContainer container)
        {
            container.ClearIngredients();
            return false;
        }

        if (item is IngredientProp ingredientProp)
        {
            Destroy(ingredientProp.gameObject);
            return true;
        }

        return false;
    
    }

    public bool OrderMatch(){
        return true;
    }
}
