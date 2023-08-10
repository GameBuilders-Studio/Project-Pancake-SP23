
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plate : FoodContainer
{
    private ModelMapper _modelMapper;

    public List<IngredientStateData> excludeState;

    public override void Start()
    {
        base.Start();
        _modelMapper = ModelMapper.Instance;
    }

    protected override bool ValidateIngredient(Ingredient ingredient)
    {
        // Check if the ingredient is allowed based on its type and progress
        if(!base.ValidateIngredient(ingredient)) {
            return false; 
        }

        if (excludeState.Contains(ingredient.State))
        {
            Debug.Log("Rejected because in contain state");
            return false;
        }

        var existingIngredients = new HashSet<(IngredientType, IngredientStateData)>(_ingredientInformation);
        if (!existingIngredients.Add((ingredient.Data.type, ingredient.State)))
        {
            Debug.Log("Rejected because cannot be added to the hashset");
            return false;
        }
        Debug.Log("Rejected because dish not exist");
        
        return ModelMapper.Instance.DishExist(existingIngredients);
    }

}
