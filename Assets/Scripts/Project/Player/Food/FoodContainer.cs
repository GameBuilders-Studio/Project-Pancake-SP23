using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodContainer : Selectable
{
    [SerializeField]
    private int _capacity;

    private int _ingredientsAdded = 0;

    public int Capacity
    {
        get => _capacity;
        set => _capacity = value;
    }

    protected override void OnAwake()
    {
        IsCarryable = true;
    }

    public override bool TryPlaceItem(Selectable item)
    {
        if (_ingredientsAdded >= Capacity) { return false; }

        var ingredient = item.GetComponent<Ingredient>();

        if (ingredient == null) { return false; }
        
        AddIngredient(ingredient);

        return true;
    }

    void AddIngredient(Ingredient ingredient)
    {
        // do something with the ingredient
        Destroy(ingredient.gameObject);
    }
}
