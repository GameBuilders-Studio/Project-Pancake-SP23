using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(fileName = "New Recipe Data", menuName = "Recipe Data")]
public class RecipeData : ScriptableObject
{
    [SerializeField]
    private string _recipeName;
    // public string description;
    [SerializeField]
    public List<IngredientSO> _recipe;

    public string RecipeName => _recipeName;
    public bool IsRecipeValid(List<IngredientSO> ingredients)
    {
        // check if the number of ingredients is correct
        if (ingredients.Count != _recipe.Count)
        {
            return false;
        }

        // check if the ingredients and states are the same using a dictionary
        var comparison = new Dictionary<IngredientSO, int>();

        foreach (var recipeIngredient in _recipe)
        {
            if (!comparison.ContainsKey(recipeIngredient))
            {
                comparison.Add(recipeIngredient, 1);
            }
            else
            {
                comparison[recipeIngredient]++;
            }
        }


        foreach (var ingredient in ingredients)
        {
            if (!comparison.ContainsKey(ingredient))
            {
                return false;
            }
            else
            {
                comparison[ingredient]--;
            }
        }

        foreach (int value in comparison.Values)
        {
            if (value != 0)
            {
                return false;
            }
        }

        return true;
    }

}
