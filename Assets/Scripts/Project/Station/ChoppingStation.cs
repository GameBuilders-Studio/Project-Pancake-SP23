using UnityEngine;

public class ChoppingStation : Station, IInteractable
{
    [Space(15f)]
    [SerializeField]
    private float _totalChopTime;

    [SerializeField]
    private IngredientStateData _targetIngredientState;

    private bool _interacting = false;
    private bool _ingredientExists = false;
    private IngredientProp _ingredient;

    bool IInteractable.Enabled
    {
        get => _ingredientExists && !_ingredient.ProgressComplete;
    }

    void Update()
    {
        if (_interacting) { Chop(); }
    }

    public void OnInteractStart() => _interacting = true;

    public void OnInteractEnd() => _interacting = false;

    protected override bool ValidatePlacedItem(Carryable item)
    {
        return true;
    }

    protected override void OnItemPlaced(Carryable item)
    {
        item.TryGetComponent(out _ingredient);
        _ingredientExists = _ingredient != null;
    }

    protected override void OnItemRemoved(Carryable item)
    {
        _ingredient = null;
        _ingredientExists = false;
    }

    void Chop()
    {
        // IInteractable.Enabled check ensures _ingredient exists
        _ingredient.Data.SetState(_targetIngredientState);
        _ingredient.AddProgress(Time.deltaTime / _totalChopTime);
    }
}
