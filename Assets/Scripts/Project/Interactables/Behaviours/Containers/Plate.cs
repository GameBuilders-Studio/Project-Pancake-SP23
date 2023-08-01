
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Plate : FoodContainer
{
    private ModelMapper _modelMapper;

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

        var existingIngredients = new HashSet<IngredientType>(_ingredientTypes);
        if (!existingIngredients.Add(ingredient.Data.type))
        {
            return false;
        }
        return ModelMapper.Instance.DishExist(existingIngredients);
    }

}
