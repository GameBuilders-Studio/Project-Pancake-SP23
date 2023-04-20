using UnityEngine;

public class CuttingBoard : StationController, IUsable
{
    [SerializeField]
    private float _chopTimeSeconds;
    [SerializeField] 
    [Tooltip("The state the ingredient has to be in to be chopped, most likely just 'raw'")]
    private IngredientStateData _startIngredientState;

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

    //You can place anything on cutting board
    public override bool ValidateItem(Carryable item)
    {
        return true;
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
        // ensure ingredient is in a state that can be chopped
        if(_ingredient.Data.State != _startIngredientState && _ingredient.Data.State != _targetIngredientState) { return; }

        // IInteractable.Enabled check ensures _ingredient exists
        if(_ingredient.ProgressComplete) { return; }

        _ingredient.Data.SetState(_targetIngredientState);
        _ingredient.AddProgress(Time.deltaTime / _chopTimeSeconds);
    }
}
