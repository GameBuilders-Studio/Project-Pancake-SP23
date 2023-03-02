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
        //Only Accept dish, don't accept ingredient
        if (item is FoodContainer container)
        {
            if (isOrderCorrect(container))
            {
                EventManager.Invoke("IncrementingScore");
            }
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
