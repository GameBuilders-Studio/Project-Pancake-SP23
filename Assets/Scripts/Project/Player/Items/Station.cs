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

        OnItemRemoved();

        return item;
    }

    public bool TryPlaceItem(Carryable newItem)
    {
        if (!ValidateItem(newItem)) { return false; }

        if (PlacedItem == null)
        {
            PlaceItem(newItem);
            return true; 
        }

        if (PlacedItem is FoodContainer container)
        {
            return container.TryAddItem(newItem);
        }

        if (newItem is FoodContainer newContainer)
        {
            // place the container if it destroys the already-placed item
            if (newContainer.TryAddItem(PlacedItem) && PlacedItem == null)
            {
                PlaceItem(newItem);
                return true;
            }
        }

        return false; 
    }

    /// <summary>
    /// Returns true if the item is allowed to be placed on the station.
    /// </summary>
    protected virtual bool ValidateItem(Carryable item)
    {
        return true;
    }

    protected virtual void OnItemPlaced(Carryable item) {}

    protected virtual void OnItemRemoved() {}

    private void PlaceItem(Carryable item)
    {
        _placedItem = item;

        CenterObject(item.gameObject);

        item.OnPlace();

        OnItemPlaced(item);
    }

    private void CenterObject(GameObject go)
    {
        go.transform.SetParent(_itemHolderPivot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }

    protected virtual void OnUpdate()
    {
        // do something with PlacedItem (cooking, etc)
    }
}
