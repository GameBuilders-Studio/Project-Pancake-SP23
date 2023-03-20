using UnityEngine;

[RequireComponent(typeof(Selectable))]
public class Station : InteractionBehaviour, ICombinable, IHasCarryable
{
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

    void OnValidate()
    {
        if (_catchTrigger == null)
        {
           _catchTrigger = ProxyTrigger.FindByName(gameObject, "CatchVolume", emitWarning: false);
        }

        if (_stationBehaviour == null)
        {
            _stationBehaviour = GetComponent<StationBehaviour>();
        }

        if (_placedItem == null) { return; }
        CenterObject(_placedItem);
    }

    void Awake()
    {
        if (_catchTrigger != null)
        {
            _catchTrigger.Enter += TryCatchItem;
        }
    }

    void Start()
    {
        if (_placedItem == null) { return; }
        PlaceItem(_placedItem);
    }

    public Carryable PopCarryable()
    {
        var item = PlacedItem;
        _stationBehaviour?.ItemRemoved(ref item);
        PlacedItem = null;
        return item;
    }

    /// <summary>
    /// Returns true if the item is placed succesfully
    /// </summary>
    public bool TryCombineWith(InteractableEntity other)
    {
        if (!other.TryGetBehaviour(out Carryable carryable)) { return false; }

        if (PlacedItem == null)
        {
            if (_stationBehaviour && !_stationBehaviour.ValidateItem(carryable))
            {
                return false; 
            }
            PlaceItem(carryable);
            return true;
        }

        if (PlacedItem.Entity.TryGetInterface(out ICombinable combinable))
        {
            return combinable.TryCombineWith(other);
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
        TryCombineWith(item.Entity);
    }

    public void PlaceItem(Carryable item)
    {
        PlacedItem = item;
        item.OnPlace();
        CenterObject(item);
        _stationBehaviour?.ItemPlaced(ref item);
    }

    private void CenterObject(Carryable item)
    {
        var go = item.gameObject;
        go.transform.SetParent(_itemHolderPivot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }
}