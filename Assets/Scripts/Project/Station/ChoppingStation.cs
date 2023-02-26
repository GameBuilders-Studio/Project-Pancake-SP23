using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoppingStation : Station, IInteractable
{
    [Space(15f)]
    [SerializeField]
    private float _totalChopTime;

    public bool IsInteractable {get; set;} = true;

    private bool _interacting = false;

    public void Chop()
    {
        if (PlacedItem is IngredientProp ingredient)
        {
            ingredient.AddProgress(Time.deltaTime / _totalChopTime);
        }
    }

    public void OnInteractStart()
    {
        _interacting = true;
    }

    public void OnInteractEnd()
    {
        _interacting = false;
    }

    protected override void OnUpdate()
    {
        if (_interacting)
        {
            Chop();
        }
    }

    protected bool ValidateItem(Selectable item)
    {
        return true;
    }
}
