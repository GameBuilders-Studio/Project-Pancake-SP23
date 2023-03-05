using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Station : Selectable
{
    [SerializeField]
    private Transform _itemHolderPivot;

    [SerializeField]
    private Carryable _placedItem;

    public Carryable PlacedItem 
    {
        get => _placedItem;
        protected set => _placedItem = value;
    }

    void Update() => OnUpdate();

    void OnValidate()
    {
        if (_placedItem == null) { return; }
        CenterObject(_placedItem.gameObject);
    }

    void Start()
    {
        if (_placedItem == null) { return; }
        PlaceItem(_placedItem);
    }

    public virtual Carryable GetCarryableItem()
    {
        var item = PlacedItem;

        PlacedItem = null;

        OnItemRemoved();

        return item;
    }

    public virtual bool TryPlaceItem(Carryable item)
    {
        if (PlacedItem == null)
        {
            if (!ValidatePlacedItem(item)) { return false; }
            PlaceItem(item);
            return true;
        }

        if (PlacedItem is FoodContainer placedContainer)
        {
            return placedContainer.TryAddItem(item);
        }

        // place the container if it destroys the placed item
        if (item is FoodContainer container)
        {
            if (container.TryAddItem(PlacedItem))
            {
                PlaceItem(item);
                return true;
            }
        }

        return false; 
    }

    /// <summary>
    /// Returns true if the item is allowed to be placed on the station.
    /// </summary>
    protected virtual bool ValidatePlacedItem(Carryable item) => true;

    protected virtual void OnItemPlaced(Carryable item) {}

    protected virtual void OnItemRemoved() {}

    protected void PlaceItem(Carryable item)
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
