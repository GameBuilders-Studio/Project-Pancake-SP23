using System.Collections.Generic;
using UnityEngine;

public class FoodContainer : InteractionBehaviour, ICombinable
{
    [SerializeField]
    private FoodContainerData _containerSettings;

    [SerializeField]
    private List<Ingredient> _ingredients = new();

    public int Count => _ingredients.Count;
    public int Capacity => _containerSettings.Capacity;

    public bool IsFull => _ingredients.Count == Capacity;
    public bool IsEmpty => _ingredients.Count == 0;

    private Dictionary<Ingredient, GameObject> _ingredientModels = new(); // Stores the models for each ingredient currently in the container

    public List<Ingredient> Ingredients
    {
        get => _ingredients;
        set => _ingredients = value;
    }

    /// <summary>
    /// Returns true if the item is destroyed when added to this container
    /// </summary>
    public bool TryCombineWith(InteractionBehaviour other)
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

    public bool TryAddIngredientProp(IngredientProp ingredientProp)
    {
        if (!ValidateIngredient(ingredientProp.Ingredient)) { return false; }

        Ingredient ingredient = ingredientProp.Ingredient;
        AddIngredient(ingredient);
        // Instantiate a model for the ingredient and display it in the container
        GameObject ingredientModel = Instantiate(ingredient.Data.platedModels[ingredient.State], transform);
        _ingredientModels.Add(ingredient, ingredientModel);
        // Position the model in the container
        ingredientModel.transform.localPosition = Vector3.zero;
        Destroy(ingredientProp.gameObject);

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
            // Move the model from the other container to this container   
            Ingredient otherIngredient = other.PopIngredient();
            // Add the model to this container
            GameObject ingredientModel = otherIngredient.Data.platedModels[otherIngredient.State];
            _ingredientModels.Add(otherIngredient, ingredientModel);
            // Position the model in the container
            ingredientModel.transform.localPosition = Vector3.zero;
            AddIngredient(otherIngredient);
            didTransfer = true;
        }

        return didTransfer;
    }

    public void ClearIngredients()
    {
        // Destroy all ingredient models
        foreach (GameObject ingredientModel in _ingredientModels.Values)
        {
            Destroy(ingredientModel);
        }
        _ingredientModels.Clear();
        Ingredients.Clear();
    }

    public Ingredient PopIngredient()
    {
        Ingredient ingredient = Ingredients[Count - 1];
        // Remove the model from this container
        GameObject ingredientModel = _ingredientModels[ingredient];
        _ingredientModels.Remove(ingredient);
        Destroy(ingredientModel);
        // Remove the ingredient from the container
        Ingredients.RemoveAt(Count - 1);
        return ingredient;
    }

    protected virtual bool ValidateIngredient(Ingredient ingredient)
    {
        return (_containerSettings.IsIngredientAllowed(ingredient) && ingredient.ProgressComplete);
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

    protected virtual void OnIngredientsChanged()
    {
        // modify visuals


    }

    private void AddIngredient(Ingredient ingredient)
    {
        Ingredients.Add(ingredient);
        Ingredients.Sort((a, b) => b.Progress.CompareTo(a.Progress));
        OnIngredientsChanged();
    }
}
