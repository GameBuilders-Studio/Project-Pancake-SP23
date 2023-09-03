using UnityEngine;
public class Stove : StationController
{
    

    [SerializeField]
    private float _overcookTime;

    [SerializeField] private float _flashFinishTime = 4f;

    private Pot _container;
    private bool _containerExists = false;
    
    private void Start () 
    {
        if(_containerExists) {
            _container.SetIsOnStove(true);
        }
    }

    public override bool ValidateItem(Carryable item)
    {
        // If there is nothing, only pot can be placed
        if(!_containerExists) { 
            return item.HasBehaviour<Pot>();
        } else {
            if (item.TryGetBehaviour(out IngredientProp ingredientProp))
            {
                return _container.ValidateIngredient(ingredientProp.Ingredient);
            }
            return false; 
        }
    }

    public override void ItemPlaced(ref Carryable item) 
    {
        _containerExists = item.TryGetBehaviour(out _container);
        if (_containerExists)
        {
            _container.SetIsOnStove(true);
        }
    }

    public override void ItemRemoved(ref Carryable item)
    {
        _containerExists = false;
        _container.SetIsOnStove(false);
        _container = null;
    }

    public void StartFire()
    {
        if (TryGetBehaviour(out Flammable flammable))
        {
            flammable.TryIgnite();
        }
    }
}
