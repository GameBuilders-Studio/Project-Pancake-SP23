using System.Collections.Generic;
using CustomAttributes;
using UnityEngine;

public class FoodContainer : InteractionBehaviour, ICombinable
{
    [SerializeField]
    private FoodContainerData _containerSettings;

    [Tooltip("If true, model for ingredients will be spawned when the container is not empty")]
    [SerializeField]
    private bool _spawnDishModel = true;

    [SerializeField]
    protected List<Ingredient> _ingredients = new();

    protected HashSet<(IngredientType, IngredientStateData)> _ingredientInformation = new();

    [SerializeField, Required]
    private Transform _ingredientModelParent;

    // Reference to the tooltip instance
    private Tooltip _tooltip;

    [SerializeField, Required]
    private GameObject _tooltipPrefab;
    public int Count => _ingredients.Count;
    public int Capacity => _containerSettings.Capacity;

    public bool IsFull => _ingredients.Count == Capacity;
    public bool IsEmpty => _ingredients.Count == 0;

    private Dictionary<Ingredient, GameObject> _ingredientModels = new(); // Stores the models for each ingredient currently in the container

    private GameObject _dishModel;

    public virtual void Start()
    {
        GameObject tooltipObject = Instantiate(_tooltipPrefab);
        _tooltip = tooltipObject.GetComponent<Tooltip>();
        _tooltip._target = gameObject.transform; // Set the target of the tooltip to this object
        Transform transform = GameObject.Find("Canvas").transform;
        if (transform == null)
        {
            Debug.LogError("Canvas not found in FoodContainer.cs");
        } else
        {
            tooltipObject.transform.SetParent(transform, false); // Set the parent of the tooltip to the HUD canvas
        }
    }

    void OnDestroy()
    {
        if (_tooltip != null)
        {
            Destroy(_tooltip.gameObject);
        }
    }

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

    private bool TryAddIngredientProp(IngredientProp ingredientProp)
    {
        if(IsFull) { return false; }
        
        if (!ValidateIngredient(ingredientProp.Ingredient))
        {
            return false;
        }

        AddIngredient(ingredientProp.Ingredient);
        Destroy(ingredientProp.gameObject);

        return true;
    }

    // protected void AddOneIngredient(IngredientProp ingredientProp)
    // {
    //     Ingredient ingredient = ingredientProp.Ingredient;
    //     AddIngredient(ingredient);
    //     // Instantiate a model for the ingredient and display it in the container
    //     GameObject ingredientModel = Instantiate(ingredientProp.StateToModel[ingredient.State], _ingredientModelParent);
    //     _ingredientModels.Add(ingredient, ingredientModel);
    //     // Position the model in the container
    //     ingredientModel.transform.localPosition = Vector3.zero;
    //     // Add the ingredient to the tooltip
    //     _tooltip.AddIngredient(ingredient.Data);
    //     Destroy(ingredientProp.gameObject);
    // }

    /// <summary>
    /// Transfer ingredients from the given container to this container
    /// </summary>
    public bool TryTransferIngredients(FoodContainer other)
    {
        if (Count >= Capacity) { return false; }

        if (!ValidateTransfer(other)) { 
            return false; }

        foreach (var otherIngredient in other.Ingredients)
        {
            AddIngredient(otherIngredient);
        }
        other.ClearIngredients();
        return true;
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
        _tooltip.ClearIngredients();
        _ingredientInformation.Clear();
        OnIngredientsChanged(_ingredientInformation);
    }

    public virtual bool ValidateIngredient(Ingredient ingredient)
    {
        bool progressComplete = ingredient.ProgressComplete;
        // if the ingredient is not allowed in this container or is not complete, return false
        if (!_containerSettings.IsIngredientAllowed(ingredient) || !progressComplete)
        {
            return false;
        } 

        return true;
    }

    /// <summary>
    /// Sets the state of the given ingredient to the given state. If the ingredient is not in the container, nothing happens.
    /// </summary>
    /// <param name="ingredient"> The ingredient to change the state of </param>
    /// <param name="state"> The target state </param>
    public void SetIngredientState(Ingredient ingredient, IngredientStateData state)
    {
        if (Ingredients.Contains(ingredient)) // Check if the ingredient is in the container
        {
            // Find the tuple that has the same ingredient type and remove it to avoid duplicates 
            _ingredientInformation.RemoveWhere(x => x.Item1 == ingredient.Data.type);

            // Change the state of the ingredient
            ingredient.SetState(state);

            // Update the ingredient information by adding the new tuple
            _ingredientInformation.Add((ingredient.Data.type, state));

            // Call OnIngredientsChanged to update the dish model and sprite
            OnIngredientsChanged(_ingredientInformation);
        }
    }

    /// <summary>
    /// Returns true if ingredient transfer from another container to this container is allowed
    /// </summary>
    protected virtual bool ValidateTransfer(FoodContainer other)
    {
        foreach (var otherIngredient in other.Ingredients)
        {
            if (!ValidateIngredient(otherIngredient))
            {
                return false;
            }
        }

        if (other._ingredientInformation.Overlaps(_ingredientInformation))
        {
            return false;
        }

        var unionSet = new HashSet<(IngredientType, IngredientStateData)>();
        unionSet.UnionWith(other._ingredientInformation);
        unionSet.UnionWith(_ingredientInformation);

        return ModelMapper.Instance.DishExist(unionSet);
    }


    protected virtual void OnIngredientsChanged(HashSet<(IngredientType, IngredientStateData)> currentIngredients)
    {
        foreach (var x in _ingredientModels.Values)
        {
            Destroy(x);
        }
        _ingredientModels.Clear();
        if (_dishModel != null)
        {
            Destroy(_dishModel);
        }

        if (currentIngredients.Count == 0)
        {
            return;
        }

        var (model, sprite) = ModelMapper.Instance.GetDishModelSprite(currentIngredients);

        if(_spawnDishModel){
            _dishModel = Instantiate(model, _ingredientModelParent);
        }
        _tooltip.ClearIngredients();
        _tooltip.SetToolTipSprite(sprite);
    }

    private void AddIngredient(Ingredient ingredient)
    {
        Ingredients.Add(ingredient);
        _ingredientInformation.Add((ingredient.Data.type, ingredient.State));
        Ingredients.Sort((a, b) => b.Progress.CompareTo(a.Progress));
        OnIngredientsChanged(_ingredientInformation);
    }
}
