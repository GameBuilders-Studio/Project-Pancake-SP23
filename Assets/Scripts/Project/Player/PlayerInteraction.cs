using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using EasyCharacterMovement;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private List<Selectable> _nearby;

    [SerializeField]
    private Carryable _heldItem = null;

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
    private IUsable _lastUsed;

    private bool _isCarrying = false;

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
        Selectable.AddListener(gameObject, _nearby);
    }

    void OnDisable()
    {
        Selectable.RemoveListener(gameObject);
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
            if (_lastUsed != null)
            {
                _lastUsed.OnUseEnd();
                _lastUsed = null;
            }
        }

        // stop interacting if interactable is disabled
        if (_lastUsed != null && !_lastUsed.Enabled)
        {
            _lastUsed.OnUseEnd();
            _lastUsed = null;
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

        if (HoverTarget.Entity.TryGetInterface(out IHasCarryable hasCarryable))
        {
            Debug.Log("pop!");
            item = hasCarryable.PopCarryable();
        }

        if (item == null) { Debug.Log("no carryable"); return; }

        PickUpItem(item);
    }

    public void TryPlace()
    {
        if (HoverTarget == null)
        {
            DropItem();
            return;
        }

        if (HoverTarget.Entity.TryGetInterface(out ICombinable combinable))
        {
            if (combinable.TryCombineWith(_heldItem.Entity))
            {
                ReleaseItem();
            }
        }
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

        item.OnPickUp();

        var go = item.gameObject;

        go.transform.parent = _carryPivot;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
    }

    public void OnUseStart()
    {
        if (HoverTarget == null) { return; }

        if (IsCarrying) { return; }

        if (HoverTarget.Entity.TryGetInterface(out IUsable usable))
        {
            if (!usable.Enabled) { return; }
            usable.OnUseStart();
            _lastUsed = usable;
        }
    }

    public void OnUseEnd()
    {
        if (_lastUsed != null)
        {
            _lastUsed.OnUseEnd();
            _lastUsed = null;
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

    private void ReleaseItem()
    {
        _character.IgnoreCollision(_heldItem.Rigidbody, ignore: false);
        _isCarrying = false;
        _heldItem = null;
    }

    private void ThrowItem()
    {
        _heldItem.OnThrow(transform.forward, _character.GetFootPosition().y, _catchTrigger.Collider);
        _heldItem.transform.parent = null;
        ReleaseItem();
    }

    private void TryCatchItem(Collider other)
    {
        if (!other.TryGetComponent(out Carryable item)) { return; }
        if (IsCarrying || !item.IsFlying) { return; }
        PickUpItem(item);
    }

    // TODO: move to separate class
    /// <summary>
    /// Chooses the best selectable based on the player's current position and rotation
    /// </summary>
    private Selectable GetBestSelectable()
    {
        static float Angle2D(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return Vector3.Angle(a, b);
        }

        Selectable bestSelectable = null;
        float minAngle = Mathf.Infinity;
        float bestScore = -1.0f;

        foreach (var nearby in Nearby)
        {
            if (!nearby.IsSelectable) { continue; }

            float angle = Angle2D(transform.forward, nearby.transform.position - transform.position);
            if (angle > _selectAngleRange) { continue; }

            float score = 0.0f;
            var entity = nearby.Entity;

            // if not carrying anything, prefer:
            // a) IUsable
            // b) IHasCarryable
            // if (!IsCarrying)
            // {
            //     bool hasUsable = entity.TryGetInterface(out IUsable _);
            //     bool hasHasCarryable = entity.TryGetInterface(out IHasCarryable _);
            //     if (hasUsable) 
            //     { 
            //         score += 2.0f; 
            //     }
            //     else if (hasHasCarryable) 
            //     {
            //         score += 1.0f; 
            //     }
            // }

            // // if carrying, prefer:
            // // a) Stations
            // // b) ICombinable
            // if (IsCarrying)
            // {
            //     bool carryingCombinable = _heldItem.Entity.HasInterface<ICombinable>();
            //     bool hasCombinable = entity.HasInterface<ICombinable>();
            //     bool hasStation = entity.HasBehaviour<Station>();
            //     if (hasStation) 
            //     { 
            //         score += 2.0f; 
            //     }
            //     else if (hasCombinable && carryingCombinable) 
            //     { 
            //         score += 1.0f; 
            //     }
            // }

            // angle is the tie-breaker
            if (angle < minAngle)
            {
                minAngle = angle;
                score += Mathf.Min(1 - (angle / _selectAngleRange), 0.999f);
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestSelectable = nearby;
            }
        }

        return bestSelectable;
    }

    private void DepenetrateHeldItem()
    {
        // use _character to resolve collision between _heldItem and walls
    }
}
