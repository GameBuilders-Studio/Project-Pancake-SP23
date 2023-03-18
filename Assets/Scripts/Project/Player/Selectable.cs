using System;
using System.Collections.Generic;
using UnityEngine;

public enum HoverState { Selected, Deselected }

public enum SelectState { Default, Disabled }

[RequireComponent(typeof(ItemBehaviourCollection))]
public class Selectable : MonoBehaviour
{
    [SerializeField]
    private ProxyTrigger _nearbyTrigger;

    [SerializeField]
    private HighlightBehaviour _highlightBehaviour;
    
    [SerializeField]
    private bool _highlightOnHover = true;

    [SerializeField]
    private ItemBehaviourCollection _interactions;

    private bool _isSelectable = true;
    private Dictionary<GameObject, PlayerInteraction> _nearbyPlayers = new();

    public ItemBehaviourCollection Interactions => _interactions;

    public virtual bool IsSelectable
    {
        get => _isSelectable;
        protected set => _isSelectable = value;
    }

    void OnValidate()
    {
        if (_nearbyTrigger == null) 
        { 
            _nearbyTrigger = ProxyTrigger.FindByName(gameObject, "NearbyVolume"); 
        }
        if (_highlightBehaviour == null) 
        { 
            _highlightBehaviour = GetComponent<HighlightBehaviour>(); 
        }
        if (_interactions == null)
        { 
            _interactions = GetComponent<ItemBehaviourCollection>(); 
        }
    }

    protected virtual void Awake()
    {
        _nearbyTrigger.Enter += OnProxyTriggerEnter;
        _nearbyTrigger.Exit += OnProxyTriggerExit;
    }
    
    public void SetSelectState(SelectState state)
    {
        if (state == SelectState.Default)
        {
            _isSelectable = true;
        }

        if (state == SelectState.Disabled)
        {
            _isSelectable = false;
            SetHoverState(HoverState.Deselected);
        }
    }

    public virtual void SetHoverState(HoverState state)
    {
        if (!(IsSelectable && _highlightOnHover))
        {
            state = HoverState.Deselected;
        }

        if (_highlightBehaviour == null) { return; }

        // TODO: highlight shader
        if (state == HoverState.Selected)
        {
            _highlightBehaviour.SetHighlight(enabled: true);
        }
        else
        {
            // disable highlight
            _highlightBehaviour.SetHighlight(enabled: false);
        }
    }

    // TODO: change collision matrix so Selectables only detect Players (for performance)
    void OnProxyTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out PlayerInteraction player))
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