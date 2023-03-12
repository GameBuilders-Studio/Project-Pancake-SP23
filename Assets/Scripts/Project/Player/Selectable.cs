using System.Collections.Generic;
using UnityEngine;

public enum HoverState { Selected, Deselected }

public enum SelectState { Default, Disabled }

public class Selectable : MonoBehaviour
{
    [SerializeField]
    private ProxyTrigger _nearbyTrigger;

    [SerializeField]
    private HighlightBehaviour _highlightBehaviour;
    
    [SerializeField]
    private bool _highlightOnHover = true;

    private bool _isSelectable = true;
    private Dictionary<GameObject, PlayerInteraction> _nearbyPlayers = new();

    public virtual bool IsSelectable
    {
        get => _isSelectable;
        protected set => _isSelectable = value;
    }

    void OnValidate() => Validate();

    void Awake() => OnAwake();
    
    public void SetState(SelectState state)
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
            _highlightBehaviour.EnableHighlight(enabled: true);
        }
        else
        {
            // disable highlight
            _highlightBehaviour.EnableHighlight(enabled: false);
        }
    }

    protected virtual void OnAwake() 
    {
        _nearbyTrigger.OnEnter += OnProxyTriggerEnter;
        _nearbyTrigger.OnExit += OnProxyTriggerExit;
    }

    protected virtual void Validate()
    {
        if (_nearbyTrigger == null)
        {
            _nearbyTrigger = ProxyTrigger.FindByName(gameObject, "NearbyVolume");
        }
        
        if (_highlightBehaviour == null)
        {
            _highlightBehaviour = GetComponent<HighlightBehaviour>();
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

public interface IInteractable
{
    public void OnInteractStart();

    public void OnInteractEnd();
 
    public bool Enabled {get;}
}