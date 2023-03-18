using UnityEngine;

public class GarbageDisposal : StationBehaviour
{
    public override void ItemPlaced(ref Carryable item)
    {
        Destroy(item.gameObject);
    }

    public override bool ValidateItem(Carryable item)
    {
        if (item.Collection.TryGetBehaviour(out FoodContainer container))
        {
            container.ClearIngredients();
            return false;
        }
        return true;
    }
}
