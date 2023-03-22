using UnityEngine;

public class CuttingBoard : StationController, IUsable
{
    [SerializeField]
    private float _chopTimeSeconds;

    [SerializeField]
    private IngredientStateData _targetIngredientState;

    private bool _interacting = false;
    private bool _ingredientExists = false;
    private IngredientProp _ingredient;

    bool IUsable.Enabled
    {
        get => _ingredientExists && !_ingredient.ProgressComplete;
    }

    void Update()
    {
        if (_interacting) { Chop(); }
    }

    public void OnUseStart() => _interacting = true;

    public void OnUseEnd() => _interacting = false;

    public override bool ValidateItem(Carryable item)
    {
        return item.HasBehaviour<IngredientProp>();
    }

    public override void ItemPlaced(ref Carryable item)
    {
        _ingredientExists = item.TryGetBehaviour(out _ingredient);
    }

    public override void ItemRemoved(ref Carryable item)
    {
        _ingredient = null;
        _ingredientExists = false;
    }

    void Chop()
    {
        // IInteractable.Enabled check ensures _ingredient exists
        _ingredient.Data.SetState(_targetIngredientState);
        _ingredient.AddProgress(Time.deltaTime / _chopTimeSeconds);
    }
}
