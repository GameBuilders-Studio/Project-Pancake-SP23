using UnityEngine;

public class IngredientDispenser : StationBehaviour
{
    [SerializeField]
    private GameObject _ingredientPrefab;

    public override void OnItemRemoved(ref Carryable item)
    {
        if (item == null)
        {
            var ingredientGo = Instantiate(_ingredientPrefab, transform.position, transform.rotation);
            item = ingredientGo.GetComponent<Carryable>();
        }
    }
}
