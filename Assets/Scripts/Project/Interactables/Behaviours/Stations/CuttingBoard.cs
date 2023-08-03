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
        if (_interacting) { 
            Debug.Log("Interacting");
            Chop(); }
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
        Debug.Log("Ingredient exists: " + _ingredientExists); 
    }

    public override void ItemRemoved(ref Carryable item)
    {
        _ingredient = null;
        _ingredientExists = false;
    }

    void Chop()
    {
        Debug.Log("Chopping");
        // ensure ingredient is in a state that can be chopped
        if (_ingredient.State != _startIngredientState && _ingredient.State != _targetIngredientState) { return; }

        // Changes the ingredient state to the target state if it isn't already
        if(_ingredient.State != _targetIngredientState)
        {
            Debug.Log("Changing state");
            _ingredient.SetState(_targetIngredientState);
        }

        // Stop chopping if the ingredient is already chopped
        if (_ingredient.ProgressComplete) { 
            Debug.Log("Progress complete");
            return; }
        
        _ingredient.AddProgress(Time.deltaTime / _chopTimeSeconds);
        
    }
}
