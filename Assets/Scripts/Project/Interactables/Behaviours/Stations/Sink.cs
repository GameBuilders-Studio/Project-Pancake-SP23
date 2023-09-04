using UnityEngine;
using CustomAttributes;
using System.Collections.Generic;

public class Sink : StationController, IUsable
{
    private float _dishWashTime;
    [SerializeField]
    private int _dirtyDishCount = 0;
    [SerializeField]
    private float _maxDishWashTime;
    [SerializeField] [ProgressBar("Dish Washing", "_maxDishWashTime", EColor.Orange)]
    private float _currentDishWashTime;
    [Required]
    [SerializeField] private DishStack _dishStack; //a dish stack to add the washed dishes to
    [Required]
    [SerializeField] private List<GameObject> _dirtyDishVisuals = new List<GameObject>(); 

    [Required]
    [SerializeField] private InGameProgress pgBar;

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
            _dirtyDishCount += _dirtyDish.Count;
            Destroy(_dirtyDish.gameObject); // destroy dirty dish
        }
        UpdateVisuals();
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
            UpdateVisuals();
        }

        _currentDishWashTime += Time.deltaTime;
        pgBar.SetProgress(Mathf.Min(1f, _currentDishWashTime / _maxDishWashTime));
    }

    private void UpdateVisuals() {
        for(int i = 0; i < _dirtyDishVisuals.Count; i++) {
            _dirtyDishVisuals[i].SetActive(i < _dirtyDishCount);
        }
    }
}
