using System.Collections.Generic;
using UnityEngine;
using EasyCharacterMovement;
using CustomAttributes;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    [Tooltip("A managed list of nearby game objects with the Selectable component")]
    private List<Selectable> _nearby;

    [SerializeField]
    [Tooltip("The item currently being held by this player.")]
    private Carryable _heldItem = null;

    [SerializeField]
    [Tooltip("Transform to place carried items in.")]
    [Required]
    private Transform _carryPivot;

    [SerializeField]
    [Tooltip("The ProxyTrigger used to detect and automatically catch thrown items.")]
    [Required]
    private ProxyTrigger _catchTrigger;

    [SerializeField]
    [Tooltip("Angle range in front of player to check for selectables.")]
    [Range(0f, 180f)]
    private float _selectAngleRange;

    private CharacterMovement _character;
    private Selectable _hoverTarget = null;
    private IUsable _lastUsed;

    [SerializeField]
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
    }

    void OnEnable()
    {
        Selectable.AddListener(gameObject, _nearby);
        _catchTrigger.Enter += TryCatchItem;
    }

    void OnDisable()
    {
        Selectable.RemoveListener(gameObject);
        _catchTrigger.Enter -= TryCatchItem;
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

        if (HoverTarget.TryGetInterface(out IHasCarryable hasCarryable))
        {
            item = hasCarryable.PopCarryable();
        }

        if (item == null) { return; }

        PickUpItem(item);
    }

    public void TryPlace()
    {
        if (HoverTarget == null)
        {
            DropItem();
            return;
        }

        if (HoverTarget.TryGetInterface(out ICombinable combinable))
        {
            if (combinable.TryCombineWith(_heldItem))
            {
                ReleaseItem();
            }
            return;
        }

        DropItem();
    }

    public void PickUpItem(Carryable item)
    {
        if (_isCarrying)
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

    /// <summary>
    /// Returns true if the player is allowed to turn in place while using.
    /// </summary>
    public bool OnUseStart()
    {
        if (_isCarrying && _heldItem.TryGetInterface(out IUsableWhileCarried heldUsable))
        {
            if (!heldUsable.Enabled) { return true; }
            heldUsable.OnUseStart();
            return true;
        }

        if (HoverTarget == null) { return false; }

        if (HoverTarget.TryGetInterface(out IUsable usable))
        {
            if (!usable.Enabled) { return false; }
            usable.OnUseStart();
            _lastUsed = usable;
        }

        return false;
    }

    public void OnUseEnd()
    {
        if (_isCarrying && _heldItem.TryGetInterface(out IUsableWhileCarried heldUsable))
        {
            heldUsable.OnUseEnd();
            return;
        }

        if (_lastUsed != null)
        {
            _lastUsed.OnUseEnd();
            _lastUsed = null;
        }
        else if (_isCarrying && _heldItem.CanThrow)
        {
            ThrowItem();
        }
    }

    public void TryDropItem()
    {
        if (IsCarrying)
        {
            DropItem();
        }
    }

    private void DropItem()
    {
        _heldItem.OnDrop();

        _heldItem.transform.parent = null;

        if (_heldItem.TryGetInterface(out IUsable usable))
        {
            usable.OnUseEnd();
        }

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

        bool carryableExists = false;
        bool usableExists = false;

        bool emptyStationExists = false;
        bool foodContainerExists = false;

        foreach (var item in Nearby)
        {
            if (!item.IsSelectable) { continue; }

            float angle = Angle2D(transform.forward, item.transform.position - transform.position);
            if (angle > _selectAngleRange) { continue; }

            // if not carrying anything, prefer:
            // a) loose Carryables or Stations with Carryables
            // b) IUsable

            if (!_isCarrying)
            {
                if (item.HasBehaviour<Carryable>()
                    || (item.TryGetBehaviour(out Station station) && station.HasItem))
                {
                    if (!carryableExists)
                    {
                        minAngle = Mathf.Infinity;
                    }
                    carryableExists = true;
                }
                else if (carryableExists)
                {
                    continue;
                }

                if (item.HasInterface<IUsable>())
                {
                    if (!usableExists)
                    {
                        minAngle = Mathf.Infinity;
                    }
                    usableExists = true;
                }
                else if (usableExists)
                {
                    continue;
                }
            }

            // if carrying an ingredient, prefer:
            // a) food containers

            // if carrying a non-ingredient, prefer:
            // b) empty Stations

            if (_isCarrying)
            {
                if (_heldItem.HasBehaviour<IngredientProp>())
                {
                    if (item.HasBehaviour<FoodContainer>())
                    {
                        if (!foodContainerExists)
                        {
                            minAngle = Mathf.Infinity;
                        }
                        foodContainerExists = true;
                    }
                    else if (foodContainerExists)
                    {
                        continue;
                    }
                }
                else
                {
                    if (item.TryGetBehaviour(out Station station) && !station.HasItem)
                    {
                        if (!emptyStationExists)
                        {
                            minAngle = Mathf.Infinity;
                        }
                        emptyStationExists = true;
                    }
                    else if (emptyStationExists)
                    {
                        continue;
                    }
                }
            }

            // use angle as the tie-breaker
            if (angle < minAngle)
            {
                minAngle = angle;
                bestSelectable = item;
            }
        }

        return bestSelectable;
    }

    private void DepenetrateHeldItem()
    {
        // use _character to resolve collision between _heldItem and walls
    }
}
