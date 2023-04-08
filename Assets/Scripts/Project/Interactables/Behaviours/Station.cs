using UnityEngine;
using CustomAttributes;

[RequireComponent(typeof(Selectable))]
public class Station : InteractionBehaviour, ICombinable, IHasCarryable
{
    [SerializeField]
    private ProxyTrigger _catchTrigger;

    [SerializeField]
    private Transform _itemHolderPivot;

    [SerializeField]
    private Carryable _placedItem;

    [Header("Dependencies")]
    [SerializeField]
    [ReadOnly, Required]
    private StationController _controller;

    public Carryable PlacedItem
    {
        get => _placedItem;
        protected set => _placedItem = value;
    }

    void OnValidate()
    {
        if (_catchTrigger == null)
        {
           _catchTrigger = ProxyTrigger.FindByName(gameObject, "CatchVolume", logNotFoundWarning: false);
        }

        if (_controller == null)
        {
            _controller = GetComponent<StationController>();
        }

        if (_placedItem == null) { return; }
        CenterObject(_placedItem);
    }

    void OnEnable()
    {
        if (_catchTrigger == null) { return; }
        _catchTrigger.Enter += TryCatchItem;
    }

    void OnDisable()
    {
        if (_catchTrigger == null) { return; }
        _catchTrigger.Enter -= TryCatchItem;
    }

    void Start()
    {
        if (_placedItem == null) { return; }
        PlaceItem(_placedItem);
    }

    public Carryable PopCarryable()
    {
        var item = PlacedItem;
        _controller.ItemRemoved(ref item);
        PlacedItem = null;
        return item;
    }

    /// <summary>
    /// Returns true if the item is placed succesfully
    /// </summary>
    public bool TryCombineWith(InteractionBehaviour other)
    {
        if (!other.TryGetBehaviour(out Carryable carryable)) { return false; }

        if (_placedItem == null)
        {
            if (!_controller.ValidateItem(carryable))
            {
                return false; 
            }
            PlaceItem(carryable);
            return true;
        }

        if (PlacedItem.TryGetInterface(out ICombinable combinable))
        {
            return combinable.TryCombineWith(other);
        }

        if (other.TryGetInterface(out ICombinable otherCombinable))
        {
            if (otherCombinable.TryCombineWith(_placedItem))
            {
                PlaceItem(carryable);
                return true;
            }
        }

        return false; 
    }

    public void PlaceItem(Carryable item)
    {
        _placedItem = item;

        item.OnPlace();

        CenterObject(item);
        
        _controller.ItemPlaced(ref item);
    }

    private void CenterObject(Carryable item)
    {
        var go = item.gameObject;
        go.transform.SetParent(_itemHolderPivot);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// Returns true if the item can be caught by the Station
    /// </summary>
    private void TryCatchItem(Collider other)
    {
        if (!other.TryGetComponent(out Carryable item)) { return; }

        if (!item.PhysicsEnabled) { return; }

        item.CancelThrow();

        TryCombineWith(item);
    }
}