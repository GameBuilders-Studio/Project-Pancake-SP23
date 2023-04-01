using UnityEngine;

public class IngredientDispenser : StationController
{
    [SerializeField]
    private IngredientSO _ingredientSO;

    public override void ItemRemoved(ref Carryable item)
    {
        if (item == null)
        {
            var ingredientGo = Instantiate(_ingredientSO.prefab, transform.position, transform.rotation);
            item = ingredientGo.GetComponent<Carryable>();
        }
    }
}
