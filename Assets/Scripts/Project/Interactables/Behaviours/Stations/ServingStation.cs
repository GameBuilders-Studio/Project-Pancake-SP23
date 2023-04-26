using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using CustomAttributes;
public class ServingStation : StationController
{
    [SerializeField, Required] private OrderSystem _orderSystem;
    [SerializeField] [Required]
    private DishStack _dishStack;
    [SerializeField]
    [Tooltip("Time delayed to spawn a new dish after submitting a dish")]
    private float _spawnTime = 2.0f;
    public override void ItemPlaced(ref Carryable item)
    {
        //Debug.Log("Called OnItemPlaced");
        Destroy(item.gameObject);
        //Spawn a new Dish on dish stack
        if(_dishStack == null){
            Debug.LogWarning("DishStack is null, not spawning dish");
            return;
        }
        StartCoroutine(SpawnDish());

    }

    public override bool ValidateItem(Carryable item)
    {
        //Only Accept dish, don't accept ingredient
        if (item.TryGetBehaviour(out FoodContainer container))
        {
            isOrderCorrect(container);
            container.ClearIngredients();
            Destroy(container.gameObject);
            return true;
        }
        return false;
    }

    public bool isOrderCorrect(FoodContainer container)
    {
        if(_orderSystem == null){
            Debug.LogWarning("OrderSystem is null, assuming order is correct");
            return true;
        }
        List<IngredientSO> ingredientDatas = new();
        foreach (var ingredient in container.Ingredients)
        {
            ingredientDatas.Add(ingredient.Data);
        }
        return _orderSystem.CheckOrderMatch(ingredientDatas);
    }

    IEnumerator SpawnDish()
    {
        yield return new WaitForSeconds(_spawnTime);
        _dishStack.Count++;
    }
}
