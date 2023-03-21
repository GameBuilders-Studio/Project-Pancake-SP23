using UnityEngine;

public class GarbageDisposal : StationController
{
    public override void ItemPlaced(ref Carryable item)
    {
        Destroy(item.gameObject);
    }

    public override bool ValidateItem(Carryable item)
    {
        if (item.Entity.TryGetBehaviour(out FoodContainer container))
        {
            container.ClearIngredients();
            return false;
        }
        return true;
    }
}
