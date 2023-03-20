using UnityEngine;

public class ServingStation : StationBehaviour
{
    public override void ItemPlaced(ref Carryable item)
    {
        Debug.Log("Called OnItemPlaced");
        Destroy(item.gameObject);
    }

    public override bool ValidateItem(Carryable item)
    {
        //Only Accept dish, don't accept ingredient
        if (item.Entity.TryGetBehaviour(out FoodContainer container))
        {
            if (isOrderCorrect(container))
            {
                EventManager.Invoke("IncrementingScore");
            }
            return true;
        }
        return false;
    }

    public bool isOrderCorrect(FoodContainer container)
    {
        return true;
    }
}
