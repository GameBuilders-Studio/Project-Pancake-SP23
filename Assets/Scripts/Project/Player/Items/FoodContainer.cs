using System.Collections.Generic;
using UnityEngine;

public class FoodContainer : Carryable
{
    [Space(15f)]
    [SerializeField]
    private int _capacity;
    
    [SerializeField]
    private List<Ingredient> _ingredients = new();

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

    public override bool IsEverThrowable 
    {
        get => false; 
    }

    public bool TryAddItem(Carryable item)
    {
        if (Count >= Capacity) { return false; }

        if (item is IngredientProp ingredientProp)
        {
            AddIngredient(ingredientProp.Data);
            OnAddIngredient();
            Destroy(ingredientProp.gameObject);
            return true;
        }

        return false;
    }

    public bool TryTransferIngredients(FoodContainer other)
    {
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
