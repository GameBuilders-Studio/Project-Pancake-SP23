using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Props/Food Container Data")]
public class FoodContainerData : ScriptableObject
{
    [SerializeField]
    public bool AllowAllIngredientTypes; 

    [SerializeField]
    [Min(0)]
    public int Capacity;

    [SerializeField]
    public List<IngredientStateData> AllowedIngredientStates;

    [SerializeField]
    public List<IngredientType> AllowedIngredientTypes = new(); 

    public virtual bool IsIngredientAllowed(Ingredient ingredient)
    {
        bool isTypeAllowed = AllowAllIngredientTypes || AllowedIngredientTypes.Contains(ingredient.Data.type); 
        bool isStateAllowed = AllowedIngredientStates.Contains(ingredient.State);
        return (isTypeAllowed && isStateAllowed); 
    }
}
