using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Station : Selectable
{
    [SerializeField]
    private Transform _itemHolderPivot;

    private Carryable _placedItem;

    public Carryable PlacedItem 
    {
        get => _placedItem; 
        set => _placedItem = value;
    }

    void Update()
    {
        OnUpdate();
    }

    public virtual Carryable GetCarryableItem()
    {
        var item = _placedItem;

        _placedItem = null;

        return item;
    }

    public bool TryPlaceItem(Carryable item)
    {
        if (_placedItem is FoodContainer container)
        {
            return container.TryAddItem(item);
        }
        else
        {
            if (!ValidateItem(item)) { return false; }

            PlaceItem(item);

            return true; 
        }
    }

    protected virtual void OnUpdate()
    {
        // do something with PlacedItem (cooking, etc)
    }

    protected virtual bool ValidateItem(Carryable item)
    {
        return true;
    }

    private void PlaceItem(Carryable item)
    {
        _placedItem = item;

        CenterObject(item.gameObject);

        item.OnPlace();
    }

    private void CenterObject(GameObject go)
    {
        go.transform.SetParent(_itemHolderPivot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
