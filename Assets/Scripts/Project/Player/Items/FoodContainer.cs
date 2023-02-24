using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodContainer : Selectable
{
    [SerializeField]
    private int _capacity;

    [SerializeField]
    private List<Ingredient> _ingredients;

    public int Count
    {
        get => _ingredients.Count;
    }

    public int Capacity
    {
        get => _capacity;
        set => _capacity = value;
    }

    public List<Ingredient> Ingredients
    {
        get => _ingredients;
        set => _ingredients = value;
    }

    protected override void OnAwake()
    {
        IsCarryable = true;
    }

    public override bool TryPlaceItem(Selectable item)
    {
        if (Count >= Capacity) { return false; }

        if (!TryAddIngredient(item)) { return false; }

        OnAddIngredient();

        return true;
    }

    protected virtual bool ValidateIngredient(Ingredient ingredient)
    {
        return ingredient != null;
    }

    protected virtual void OnAddIngredient()
    {
        // change visuals
    }

    private bool TryAddIngredient(Selectable item)
    {
        if (item.TryGetComponent(out IngredientProp ingredientProp))
        {
            Ingredients.Add(ingredientProp.Data);
            Destroy(ingredientProp.gameObject);
            return true;
        }

        if (item.TryGetComponent(out FoodContainer foodContainer))
        {
            foreach (Ingredient ingredient in foodContainer.Ingredients)
            {
                Ingredients.Add(ingredient);
            }
            return true;
        }

        return false;
    }
}
