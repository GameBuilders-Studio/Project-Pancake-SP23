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
        return true;
    }
}
