using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngredientDispenser : Station
{
    [SerializeField]
    private GameObject _ingredientPrefab;

    public override Carryable PopCarryableItem()
    {
        var item = PlacedItem;

        if (item == null)
        {
            var ingredientGo = Instantiate(_ingredientPrefab, transform.position, transform.rotation);
            item = ingredientGo.GetComponent<Carryable>();
        }

        PlacedItem = null;

        return item;
    }
}
