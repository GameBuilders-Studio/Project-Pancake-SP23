using UnityEngine;
using CustomAttributes;

public class IngredientDispenser : StationController
{
    [SerializeField]
    private IngredientSO _ingredientSO;

    [SerializeField, Required]
    private SpriteRenderer _iconRenderer;

    private void OnValidate()
    {
        _iconRenderer.sprite = _ingredientSO.icon;
    }

    public override void ItemRemoved(ref Carryable item)
    {
        if (item == null)
        {
            var ingredientGo = Instantiate(_ingredientSO.prefab, transform.position, transform.rotation);
            item = ingredientGo.GetComponent<Carryable>();
        }
    }
}
