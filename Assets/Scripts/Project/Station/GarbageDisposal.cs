using UnityEngine;

public class GarbageDisposal : StationBehaviour
{
    public override void OnItemPlaced(ref Carryable item)
    {
        Destroy(item.gameObject);
    }

    public override bool ValidateItem(Carryable item)
    {
        if (item is FoodContainer container)
        {
            container.ClearIngredients();
            return false;
        }
        return true;
    }
}
