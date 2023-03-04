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

    public void Chop()
    {
        _ingredient.AddProgress(Time.deltaTime / _totalChopTime);
    }

    public void OnInteractStart()
    {
        _interacting = true;
        _ingredient.SetIngredientState(_newIngredientState);
    }

    public void OnInteractEnd()
    {
        _interacting = false;
    }

    protected override void OnItemPlaced(Carryable item)
    {
        _ingredient = PlacedItem as IngredientProp;
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

    protected override bool ValidateItem(Carryable item)
    {
        return true;
    }
}
