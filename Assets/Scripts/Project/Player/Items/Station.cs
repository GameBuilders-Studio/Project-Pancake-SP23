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

        var newItemContainer = newItem as FoodContainer;

        if (PlacedItem is FoodContainer placedContainer)
        {
            if (newItemContainer != null)
            {
                placedContainer.TryTransferIngredients(newItemContainer);
                return false;
            }
            return placedContainer.TryAddItem(newItem);
        }

        // place the container if it destroys the placed item
        if (newItemContainer != null)
        {
            if (newItemContainer.TryAddItem(PlacedItem))
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
        PlacedItem = item;

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
