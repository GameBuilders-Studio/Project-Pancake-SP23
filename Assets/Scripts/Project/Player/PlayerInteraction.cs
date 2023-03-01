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

    private CharacterMovement _character;

    private Selectable _hoverTarget = null;
    private Carryable _currentHeldItem = null;
    private IInteractable _lastInteracted;

    RaycastHit[] _results = new RaycastHit[1];

    public List<Selectable> Nearby
    {
        get => _nearby;
        set => _nearby = value;
    }

    public Selectable HoverTarget
    {
        get => _hoverTarget;
        set => _hoverTarget = value;
    }

    public bool IsCarrying
    {
        get => _currentHeldItem != null;
    }

    void Awake()
    {
        Nearby = new();
        _character = GetComponent<CharacterMovement>();
    }

    void Update()
    {
        if (IsCarrying)
        {
            DepenetrateHeldItem();
        }

        var selectable = GetBestSelectable();

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
        
        if (HoverTarget is Station station)
        {
            item = station.GetCarryableItem();
        }

        if (item == null) { return; }

        item.OnPickUp();

        PickUpItem(item);
    }

    public void TryPlace()
    {
        if (HoverTarget is Station station)
        {
            if (station.TryPlaceItem(_currentHeldItem))
            {
                if (_currentHeldItem != null)
                {
                    _currentHeldItem.OnPlace();
                    _currentHeldItem = null;
                }
            }
            return; // keep holding items if a station is selected
        }

        if (HoverTarget is FoodContainer container)
        {
            if (container.TryAddItem(_currentHeldItem))
            {
                return; 
            }
        }

        DropItem();
    }

    public void PickUpItem(Carryable item)
    {
        if (_currentHeldItem != null)
        {
            Debug.LogError("Tried to pick up item while already carrying one");
            return;
        }

        _currentHeldItem = item;
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

        if (HoverTarget is IInteractable interactable)
        {
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
        else if (IsCarrying && _currentHeldItem.CanThrow)
        {
            ThrowItem();
        }
    }

    private void DropItem()
    {
        _currentHeldItem.OnDrop();
        ReleaseItem();
    }

    private void ThrowItem()
    {
        _currentHeldItem.OnThrow(transform.forward, _character.GetFootPosition().y);
        ReleaseItem();
    }

    private void ReleaseItem()
    {
        _currentHeldItem.transform.parent = null;
        _character.IgnoreCollision(_currentHeldItem.Rigidbody, false);
        _currentHeldItem = null;
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
        var startPosition = transform.position;
        startPosition.y = _carryPivot.position.y + 0.4f;

        float maxDistance = Vector3.Distance(startPosition, _carryPivot.position);

        // if (Physics.SphereCastNonAlloc(startPosition, _character.radius, transform.forward, _results, maxDistance) > 0)
        // {
        //     float overlapDistance = maxDistance - _results[0].distance;
        //     _character.SetPosition(transform.position + (-transform.forward * overlapDistance));
        // }

        if (_character.MovementSweepTest(transform.position + Vector3.up, transform.forward, maxDistance, out CollisionResult result))
        {
            float overlapDistance = maxDistance - result.displacementToHit.magnitude;
            _character.SetPosition(transform.position + -transform.forward * overlapDistance);
        }
    }
}
