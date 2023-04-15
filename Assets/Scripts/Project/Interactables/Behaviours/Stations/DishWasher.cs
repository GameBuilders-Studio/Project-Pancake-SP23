using UnityEngine;

public class DishWasher : StationController, IUsable
{
    [SerializeField]
    private float _dishWashTime;
    [SerializeField]
    private int _dirtyDishCount = 0;
    [SerializeField]
    private DishWasherSO _dishWasherSO;
    [SerializeField]
    private float _currentProgress;
    [SerializeField]
    private DishStack _dishStack;

    private bool _interacting = false;
    private bool _ingredientExists = false;

    bool IUsable.Enabled
    {
        get => _ingredientExists && !_ingredient.ProgressComplete;
    }

    void Update()
    {
        if (_interacting) { Wash(); }
    }

    public void OnUseStart() => _interacting = true;

    public void OnUseEnd() => _interacting = false;

    public override bool ValidateItem(Carryable item)
    {
        return item.HasBehaviour<IngredientProp>();
    }

    public override void ItemPlaced(ref Carryable item)
    {
        DirtyDish _dirtyDish;
        _ingredientExists = item.TryGetBehaviour(out _dirtyDish);
        if (_ingredientExists)
        {
            _dirtyDishCount = _dirtyDish.Count;
            Destroy(_dirtyDish.gameObject); // destroy dirty dish
        }
    }

    public override void ItemRemoved(ref Carryable item)
    {
        //do nothing
        //you can't remove anything from the dish washer
    }

    void Wash()
    {
        // ensure ingredient is in a state that can be chopped
        if(_dirtyDishCount <=0 && _ingredient.Data.State != _targetIngredientState) { return; }

        // IInteractable.Enabled check ensures _ingredient exists
        if(_dishWasherSO.dishWashTime>=_currentProgress) {
            _currentProgress = 0;
            _dirtyDishCount--;
            _dishStack.CurrentPlates++;
        }

        _currentProgress+=Time.deltaTime / _dishWashTime;
    }
}
