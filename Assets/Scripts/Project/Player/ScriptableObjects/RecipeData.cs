using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

[CreateAssetMenu(fileName = "New Recipe Data", menuName = "Recipe Data")]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    // public string description;
    public List<IngredientType> recipe;

    public bool IsRecipeValid(List<IngredientType> ingredients)
    {
        // check if the number of ingredients is correct
        if (ingredients.Count != recipe.Count)
        {
            return false;
        }

        // check if the ingredients and states are the same using a dictionary
        var comparison = new Dictionary<IngredientType, int>();

        foreach (var recipeIngredient in recipe)
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
