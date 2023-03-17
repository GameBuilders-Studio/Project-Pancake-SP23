using System.Collections.Generic;
using UnityEngine;
using EasyCharacterMovement;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private List<Selectable> _nearby;

    [Tooltip("Transform to place carried items in")]
    [SerializeField]
    private Transform _carryPivot;

    [Tooltip("Radius used to depenetrate carried item from walls")]
    [SerializeField]
    private float _carryPivotRadius;

    [Tooltip("Angle range in front of player to check for selectables")]
    [Range(0f, 180f)]
    [SerializeField]
    private float _selectAngleRange;

    [SerializeField]
    private ProxyTrigger _catchTrigger;

    private CharacterMovement _character;
    private Selectable _hoverTarget = null;
    private IInteractable _lastInteracted;
    
    private bool _isCarrying = false;
    private Carryable _heldItem = null;

    public bool IsCarrying => _isCarrying;

    public List<Selectable> Nearby
    {
        get => _nearby;
        private set => _nearby = value;
    }

    public Selectable HoverTarget
    {
        get => _hoverTarget;
        private set => _hoverTarget = value;
    }

    void OnValidate()
    {
        if (_catchTrigger != null) { return; }
        _catchTrigger = ProxyTrigger.FindByName(gameObject, "CatchVolume");
    }

    void Awake()
    {
        Nearby = new();
        _character = GetComponent<CharacterMovement>();
        _catchTrigger.Enter += TryCatchItem;
    }

    void Update()
    {
        if (IsCarrying)
        {
            DepenetrateHeldItem();
        }

        var selectable = GetBestSelectable();
        
        // deselect previous target
        if (HoverTarget != null)
        {
            HoverTarget.SetHoverState(HoverState.Deselected);
        }
        
        if (selectable != null)
        {
            HoverTarget = selectable;
            HoverTarget.SetHoverState(HoverState.Selected);
        }
        else
        {
            HoverTarget = null;
            // stop interacting if nothing is selected
            if (_lastInteracted != null)
            {
                _lastInteracted.OnInteractEnd();
                _lastInteracted = null;
            }
        }

        // stop interacting if interactable is disabled
        if (_lastInteracted != null && !_lastInteracted.Enabled)
        {
            _lastInteracted.OnInteractEnd();
            Debug.Log("interact canceled");
            _lastInteracted = null;
        }
    }

    public void OnPickUpPressed()
    {
        if (IsCarrying)
        {
            TryPlace();
        }
        else
        {
            TryPickUp();
        }
    }

    public void TryPickUp()
    {
        if (HoverTarget == null) { return; }

        Carryable item = null;

        if (HoverTarget is Carryable carryable)
        {
            item = carryable;
        }
        else if (HoverTarget is Station station)
        {
            item = station.PopCarryableItem();
        }

        if (item == null) { return; }

        item.OnPickUp();

        PickUpItem(item);
    }

    public void TryPlace()
    {
        if (HoverTarget is Station station)
        {
            if (IsCarrying && station.TryPlaceItem(_heldItem))
            {
                _heldItem.OnPlace();
                ReleaseItem();
            }
            return; // keep holding items if a station is selected
        }

        if (HoverTarget is FoodContainer container)
        {
            if (container.TryAddItem(_heldItem))
            {
                ReleaseItem();
            }
            return; // keep holding item if a container is selected
        }

        DropItem();
    }

    public void PickUpItem(Carryable item)
    {
        if (IsCarrying)
        {
            Debug.LogError("Tried to pick up item while already carrying one");
            return;
        }

        _isCarrying = true;

        _heldItem = item;
        _character.IgnoreCollision(item.Rigidbody);

        var go = item.gameObject;

        go.transform.parent = _carryPivot;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }

    public void OnInteractStart()
    {
        if (HoverTarget == null) { return; }

        if (IsCarrying) { return; }

        if (HoverTarget.TryGetComponent(out IInteractable interactable))
        {
            if (!interactable.Enabled) { return; }
            interactable.OnInteractStart();
            _lastInteracted = interactable;
        }
    }

    public void OnInteractEnd()
    {
        if (_lastInteracted != null)
        {
            _lastInteracted.OnInteractEnd();
            _lastInteracted = null;
        }
        else if (IsCarrying && _heldItem.CanThrow)
        {
            ThrowItem();
        }
    }

    private void DropItem()
    {
        _heldItem.OnDrop();
        _heldItem.transform.parent = null;
        ReleaseItem();
    }

    private void ThrowItem()
    {
        _heldItem.OnThrow(transform.forward, _character.GetFootPosition().y);
        _heldItem.transform.parent = null;
        ReleaseItem();
    }

    private void ReleaseItem()
    {
        _character.IgnoreCollision(_heldItem.Rigidbody, false);
        _isCarrying = false;
        _heldItem = null;
    }

    private void TryCatchItem(Collider other)
    {
        if (!other.TryGetComponent(out Carryable item)) { return; }
        if (IsCarrying || !item.IsFlying) { return; }
        PickUpItem(item);
    }

    /// <summary>
    /// Gets a selectable that the player is facing
    /// </summary>
    private Selectable GetBestSelectable()
    {
        static float Angle2D(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Angle(a, b);
        }

        Selectable nearest = null;
        
        float minAngle = Mathf.Infinity;
        
        for (int i = 0; i < Nearby.Count; i++)
        {
            if (!Nearby[i].IsSelectable) { continue; }

            float angle = Angle2D(transform.forward, Nearby[i].transform.position - transform.position);

            if (angle < minAngle && angle < _selectAngleRange)
            {
                nearest = Nearby[i];
                minAngle = angle;
            }
        }

        return nearest;
    }

    private void DepenetrateHeldItem()
    {
        // use _character to resolve collision between _heldItem and walls
    }
}
