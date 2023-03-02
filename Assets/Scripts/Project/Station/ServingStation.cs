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
            container.ClearIngredients();
            Destroy(container.gameObject);
            if (OrderMatch())
            {
                EventManager.Invoke("IncrementingScore");
            }
            return true;
        }
        return false;

    }

    public bool OrderMatch()
    {
        return true;
    }
}
