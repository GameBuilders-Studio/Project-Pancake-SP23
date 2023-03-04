using UnityEngine;

public class ChoppingStation : Station, IInteractable
{
    [Space(15f)]
    [SerializeField]
    private float _totalChopTime;

    [SerializeField]
    private IngredientStateData _newIngredientState;

    private bool _interacting = false;
    private IngredientProp _ingredient;

    public bool Enabled 
    {
        get => _ingredient != null && !_ingredient.ProgressComplete;
    }

    public void OnInteractStart()
    {
        _interacting = true;
    }

    public void OnInteractEnd()
    {
        _interacting = false;
    }

    protected override bool ValidateItem(Carryable item)
    {
        return true;
    }

    protected override void OnItemPlaced(Carryable item)
    {
        item.TryGetComponent(out _ingredient);
    }

    protected override void OnItemRemoved()
    {
        _ingredient = null;
    }

    protected override void OnUpdate()
    {
        if (_interacting)
        {
            Chop();
        }
    }

    void Chop()
    {
        _ingredient.SetIngredientState(_newIngredientState);
        _ingredient.AddProgress(Time.deltaTime / _totalChopTime);
    }
}
