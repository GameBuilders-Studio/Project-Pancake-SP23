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

    protected Rigidbody _rigidbody;

    private bool _isSelectable = true;

    private Dictionary<GameObject, PlayerInteraction> _nearbyPlayers = new();

    // TODO: remove
    private Renderer _renderer;

    public virtual bool IsSelectable
    {
        get => _isSelectable && _isEverSelectable;
        set => _isSelectable = value;
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

public interface IInteractable
{
    public void OnInteractStart();
    public void OnInteractEnd();
    public bool IsInteractable {get; set;}
}