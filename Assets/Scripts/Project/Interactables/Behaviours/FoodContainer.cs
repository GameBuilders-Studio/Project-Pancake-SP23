using System.Collections.Generic;
using UnityEngine;

public class FoodContainer : InteractionBehaviour, ICombinable
{
    [SerializeField]
    private int _capacity;
    
    [SerializeField]
    private List<Ingredient> _ingredients = new();

    public int Count => _ingredients.Count;
    public int Capacity => _capacity;

    public bool IsFull => _ingredients.Count == _capacity;
    public bool IsEmpty => _ingredients.Count == 0;

    public List<Ingredient> Ingredients
    {
        get => _ingredients;
        set => _ingredients = value;
    }

    /// <summary>
    /// Returns true if the item is destroyed when added to this container
    /// </summary>
    public bool TryCombineWith(InteractionProvider other)
    {
        if (other.TryGetBehaviour(out IngredientProp ingredientProp))
        {
            return TryAddIngredientProp(ingredientProp);
        }

        if (other.TryGetBehaviour(out FoodContainer foodContainer))
        {
            if (IsEmpty && !foodContainer.IsEmpty)
            {
                TryTransferIngredients(foodContainer);
            }
            else if (!IsEmpty && foodContainer.IsEmpty)
            {
                foodContainer.TryTransferIngredients(this);
            }
            else
            {
                TryTransferIngredients(foodContainer);
            }

            return false;
        }
        
        return false;
    }

    public bool TryAddIngredientProp(IngredientProp ingredient)
    {
        if (!ValidateIngredient(ingredient.Data)) { return false; }

        AddIngredient(ingredient.Data);
        OnAddIngredient();
        Destroy(ingredient.gameObject);
        
        return true;
    }

    /// <summary>
    /// Transfer ingredients from the given container to this container 
    /// </summary>
    public bool TryTransferIngredients(FoodContainer other)
    {
        if (Count >= Capacity) { return false; }
        
        if (!ValidateTransfer(other)) { return false; }
        
        bool didTransfer = false;

        while (Count < Capacity && other.Count > 0)
        {
            AddIngredient(other.PopIngredient());
            didTransfer = true;
        }

        return didTransfer;
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
        // modify visuals
    }

    private void AddIngredient(Ingredient ingredient)
    {
        Ingredients.Add(ingredient);
        Ingredients.Sort((a, b) => b.Progress.CompareTo(a.Progress));
    }
}
