using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Station : Selectable
{
    [Space(15f)]
    [SerializeField]
    private StationBehaviour _stationBehaviour;

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

    protected override void Validate()
    {
        base.Validate();

        if (_catchTrigger == null)
        {
           _catchTrigger = ProxyTrigger.FindByName(gameObject, "CatchVolume");
        }
        if (_stationBehaviour == null)
        {
            _stationBehaviour = GetComponent<StationBehaviour>();
        }

        if (_placedItem == null) { return; }
        CenterObject(_placedItem);
    }

    protected override void OnAwake()
    {
        base.OnAwake();
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
        _stationBehaviour?.OnItemRemoved(ref item);
        PlacedItem = null;
        return item;
    }

    /// <summary>
    /// Returns true if the item is placed succesfully
    /// </summary>
    public virtual bool TryPlaceItem(Carryable item)
    {
        if (PlacedItem == null)
        {
            if (_stationBehaviour && !_stationBehaviour.ValidateItem(item)) 
            {
                return false; 
            }
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

    public void PlaceItem(Carryable item)
    {
        PlacedItem = item;
        item.OnPlace();
        CenterObject(item);
        _stationBehaviour?.OnItemPlaced(ref item);
    }

    private void CenterObject(Carryable item)
    {
        var go = item.gameObject;
        go.transform.SetParent(_itemHolderPivot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}
