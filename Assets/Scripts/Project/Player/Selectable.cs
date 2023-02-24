using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum HoverState {Selected, Deselected}

public enum SelectableState {Default, Disabled}

[RequireComponent(typeof(Rigidbody))]
public class Selectable : MonoBehaviour
{
    [SerializeField]
    private ProxyTrigger _nearbyTrigger;

    [Space(15f)]
    [SerializeField]
    private bool _isEverSelectable = true;

    [SerializeField]
    private bool _highlightOnHover = true;

    private bool _isSelectable = true;

    private bool _isCarryable = true;

    private bool _isInteractable = false;
    private bool _isInteracting = false;

    private Dictionary<GameObject, PlayerInteraction> _nearbyPlayers = new();

    // TODO: remove
    private Renderer _renderer;
    private Rigidbody _rigidbody;

    public virtual bool IsSelectable
    {
        get => _isSelectable && _isEverSelectable;
        set => _isSelectable = value;
    }

    public virtual bool IsCarryable
    {
        get => _isCarryable;
        set => _isCarryable = value;
    }

    public virtual bool IsInteractable
    {
        get => _isInteractable;
        set => _isInteractable = value;
    }

    public virtual bool IsInteracting
    {
        get => _isInteracting && IsInteractable;
        set => _isInteracting = value;
    }

    void Awake()
    {
        if (_nearbyTrigger == null)
        {
            _nearbyTrigger = GetComponentInChildren<ProxyTrigger>();
        }

        _rigidbody = GetComponent<Rigidbody>();

        _nearbyTrigger.OnEnter += OnProxyTriggerEnter;
        _nearbyTrigger.OnExit += OnProxyTriggerExit;

        // TODO: remove
        _renderer = GetComponent<Renderer>();

        OnAwake();
    }

    public void SetState(SelectableState state)
    {
        if (state == SelectableState.Default)
        {
            _isSelectable = true;
        }

        if (state == SelectableState.Disabled)
        {
            _isSelectable = false;
            SetHoverState(HoverState.Deselected);
        }
    }

    public virtual void SetHoverState(HoverState state)
    {
        if (!IsSelectable || !_highlightOnHover)
        {
            state = HoverState.Deselected;
        }

        // TODO: highlight shader
        if (state == HoverState.Selected)
        {
            // enable highlight
            _renderer.material.color = Color.red;
        }
        else
        {
            // disable highlight
            _renderer.material.color = Color.white;
        }
    }

    /// <summary>
    /// Returns an item to be carried.
    /// </summary>
    public virtual Selectable GetCarryableItem()
    {
        return IsCarryable ? this : null;
    }

    /// <summary>
    /// Tries to place an item on this. Returns true if successful. 
    /// </summary>
    public virtual bool TryPlaceItem(Selectable item)
    {
        return false;
    }

    public virtual void OnPickUp()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = false;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
        SetState(SelectableState.Default);
    }

    /// <summary>
    /// Disables the selectable and physics by default
    /// </summary>
    public virtual void OnPlace()
    {
        _rigidbody.isKinematic = true;
        _rigidbody.detectCollisions = false;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
        SetState(SelectableState.Disabled);
    }

    public virtual void OnInteractStart() 
    {
        _isInteracting = true;
    }

    public virtual void OnInteractEnd() 
    {
        _isInteracting = false;
    }

    protected virtual void OnAwake() {}

    // TODO: change collision matrix so Selectables only detect Players (for performance)
    void OnProxyTriggerEnter(Collider other)
    {
        if (IsSelectable && other.gameObject.TryGetComponent(out PlayerInteraction player))
        {
            _nearbyPlayers.Add(other.gameObject, player);
            player.Nearby.Add(this);
        }
    }

    void OnProxyTriggerExit(Collider other)
    {
        if (_nearbyPlayers.ContainsKey(other.gameObject))
        {
            var player = _nearbyPlayers[other.gameObject];
            player.Nearby.Remove(this);
            _nearbyPlayers.Remove(other.gameObject);
        }
    }
}
