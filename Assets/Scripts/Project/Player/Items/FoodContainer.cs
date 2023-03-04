using System.Collections.Generic;
using UnityEngine;

public class FoodContainer : Carryable
{
    [Space(15f)]
    [SerializeField]
    private int _capacity;
    
    [SerializeField]
    private List<Ingredient> _ingredients = new();

    public int Count => _ingredients.Count;

    public int Capacity => _capacity;

    public bool IsFull => _ingredients.Count == _capacity;

    public List<Ingredient> Ingredients
    {
        get => _ingredients;
        set => _ingredients = value;
    }

    public override bool IsEverThrowable => false;

    /// <summary>
    /// Returns true if the item is destroyed when added to this container
    /// </summary>
    public bool TryAddItem(Carryable item)
    {
        if (Count >= Capacity) { return false; }

        if (item.TryGetComponent(out IngredientProp ingredientProp))
        {
            return TryAddIngredient(ingredientProp);
        }

        if (item is FoodContainer foodContainer)
        {
            TryTransferIngredients(foodContainer);
            return false;
        }
        
        return false;
    }

    public bool TryAddIngredient(IngredientProp ingredient)
    {
        if (!ValidateIngredient(ingredient.Data)) { return false; }

        AddIngredient(ingredient.Data);
        OnAddIngredient();
        Destroy(ingredient.gameObject);
        
        return true;
    }

    /// <summary>
    /// Try to transfer ingredients from the other container to this container
    /// </summary>
    public bool TryTransferIngredients(FoodContainer other)
    {
        if (!ValidateTransfer(other)) { return false; }
        
        bool ingredientsTransfered = false;

        while (Count < Capacity && other.Count > 0)
        {
            AddIngredient(other.PopIngredient());
            ingredientsTransfered = true;
        }

        return ingredientsTransfered;
    }

    public void ClearIngredients()
    {
        Ingredients.Clear();
    }

    public Ingredient PopIngredient()
    {
        Ingredient ingredient = Ingredients[Count - 1];
        Ingredients.RemoveAt(Count - 1);
        return ingredient;
    }

    protected virtual bool ValidateIngredient(Ingredient ingredient)
    {
        return ingredient != null;
    }

    /// <summary>
    /// Returns true if ingredient transfer from another container to this container is allowed
    /// </summary>
    protected virtual bool ValidateTransfer(FoodContainer other)
    {
        // validate each ingredient by default
        for (int i = 0; i < other.Count; i++)
        {
            if (!ValidateIngredient(other.Ingredients[i])) { return false; }
        }
        return true;
    }

    protected virtual void OnAddIngredient()
    {
        // change visuals
    }

    private void AddIngredient(Ingredient ingredient)
    {
        Ingredients.Add(ingredient);
        Ingredients.Sort((a, b) => b.Progress.CompareTo(a.Progress));
    }
}
