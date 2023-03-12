using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Station : Selectable
{
    [Space(15f)]
    [SerializeField]
    private ProxyTrigger _catchTrigger;

    [SerializeField]
    private Transform _itemHolderPivot;

    [SerializeField]
    private Carryable _placedItem;

    public Carryable PlacedItem 
    {
        get => _placedItem;
        protected set => _placedItem = value;
    }

    protected override void OnOnValidate()
    {
        base.OnOnValidate();

        if (_catchTrigger == null)
        {
           _catchTrigger = ProxyTrigger.FindByName(gameObject, "CatchVolume");
        }

        if (_placedItem == null) { return; }
        CenterObject(_placedItem);
    }

    protected override void OnAwake()
    {
        _catchTrigger.OnEnter += TryCatchItem;
    }

    void Start()
    {
        if (_placedItem == null) { return; }
        PlaceItem(_placedItem);
    }

    public virtual Carryable PopCarryableItem()
    {
        var item = PlacedItem;
        PlacedItem = null;
        OnItemRemoved(item);
        return item;
    }

    /// <summary>
    /// Returns true if the item is placed succesfully
    /// </summary>
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
    /// Returns true if the item can be caught by the Station
    /// </summary>
    public void TryCatchItem(Collider other)
    {
        if (!other.TryGetComponent(out Carryable item)) { return; }
        if (!item.PhysicsEnabled) { return; }
        item.CancelThrow();
        TryPlaceItem(item);
    }

    /// <summary>
    /// Returns true if the item is allowed to be placed on the station.
    /// </summary>
    protected virtual bool ValidatePlacedItem(Carryable item) => true;

    protected virtual void OnItemPlaced(Carryable item) {}

    protected virtual void OnItemRemoved(Carryable item) {}

    protected void PlaceItem(Carryable item)
    {
        PlacedItem = item;
        item.OnPlace();
        CenterObject(item);
        OnItemPlaced(item);
    }

    private void CenterObject(Carryable item)
    {
        var go = item.gameObject;
        go.transform.SetParent(_itemHolderPivot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
