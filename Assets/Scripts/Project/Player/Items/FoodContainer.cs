using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodContainer : Carryable
{
    [SerializeField]
    private int _capacity;

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

    public bool TryAddItem(Carryable item)
    {
        if (Count >= Capacity) { return false; }

        if (item is IngredientProp ingredientProp)
        {
            AddIngredientProp(ingredientProp);
            OnAddIngredient();
        }
        
        if (item is FoodContainer foodContainer)
        {
            if (TryTransferIngredients(foodContainer))
            {
                OnAddIngredient();
            }
        }

        return true;
    }

    public bool TryTransferIngredients(FoodContainer other)
    {
        bool ingredientsTransfered = false;

        while (Count >= Capacity && other.Count > 0)
        {
            Ingredient ingredient = other.Ingredients[Count - 1];

            other.Ingredients.RemoveAt(Count - 1);

            Ingredients.Add(ingredient);
            
            ingredientsTransfered = true;
        }

        return ingredientsTransfered;
    }

    protected virtual bool ValidateIngredient(Ingredient ingredient)
    {
        return ingredient != null;
    }

    protected virtual void OnAddIngredient()
    {
        // change visuals
    }

    private bool AddIngredientProp(IngredientProp ingredientProp)
    {
        Ingredients.Add(ingredientProp.Data);
        Destroy(ingredientProp.gameObject);
        return true;
    }
}
