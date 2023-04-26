using UnityEngine;
using CustomAttributes;

public class Sink : StationController, IUsable
{
    private float _dishWashTime;
    [SerializeField]
    private int _dirtyDishCount = 0;
    [SerializeField]
    private float _maxDishWashTime;
    [SerializeField] [ProgressBar("Dish Washing", "_maxDishWashTime", EColor.Orange)]
    private float _currentDishWashTime;
    [SerializeField] [Required]
    private DishStack _dishStack; //a dish stack to add the washed dishes to

    private bool _interacting = false;
    private bool _ingredientExists = false;

    bool IUsable.Enabled
    {
        get => _ingredientExists && _dirtyDishCount > 0;
    }

    void Update()
    {
        if (_interacting)
        {
            Wash();
        }
    }

    public void OnUseStart() => _interacting = true;

    public void OnUseEnd() => _interacting = false;

    public override bool ValidateItem(Carryable item)
    {
        return item.HasBehaviour<DirtyPlate>();
    }

    public override void ItemPlaced(ref Carryable item)
    {

        DirtyPlate _dirtyDish;
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
        if (_dirtyDishCount <= 0) { return; }

        // IInteractable.Enabled check ensures _ingredient exists
        if (_currentDishWashTime>=_maxDishWashTime)
        {
            _currentDishWashTime = 0;
            _dirtyDishCount--;
            _dishStack.Count++;
        }

        _currentDishWashTime += Time.deltaTime / _maxDishWashTime;
    }
}
