using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Props/Food Container Data")]
public class FoodContainerData : ScriptableObject
{
    [SerializeField]
    public int Capacity;

    [SerializeField]
    public List<IngredientStateData> AllowedIngredientStates;

    public virtual bool IsIngredientAllowed(Ingredient ingredient)
    {
        return AllowedIngredientStates.Contains(ingredient.State);
    }
}
