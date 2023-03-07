using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HoverState
{
    Selected,
    Deselected
}

public enum SelectState
{
    Default,
    Disabled
}

[RequireComponent(typeof(Rigidbody))]
public class Selectable : MonoBehaviour
{
    [SerializeField]
    private ProxyTrigger _nearbyTrigger;

    [SerializeField]
    private Interactable _interactable;

    [Space(15f)]
    [SerializeField]
    private bool _isCarryable = true;

    [SerializeField]
    private bool _isEverSelectable = true;

    [SerializeField]
    private bool _highlightOnHover = true;

    private bool _isSelectable = true;
    private Dictionary<GameObject, PlayerInteraction> _nearbyPlayers = new();

    // TODO: remove
    private Renderer _renderer;

    protected Material _material;

    public Material _selectMaterial;

    public virtual bool IsSelectable
    {
        get => _isSelectable && _isEverSelectable;
        set => _isSelectable = value;
    }

    public Interactable Interactable
    {
        get => _interactable;
        set => _interactable = value;
    }

    public bool IsCarryable
    {
        get => _isCarryable;
        set => _isCarryable = value;
    }

    void Awake()
    {
        if (_nearbyTrigger == null)
        {
            _nearbyTrigger = GetComponentInChildren<ProxyTrigger>();
        }

        if (_nearbyTrigger == null)
        {
            Debug.LogError("No proxy trigger");
        }

        if (_interactable == null)
        {
            _interactable = GetComponent<Interactable>();
        }

        _nearbyTrigger.OnEnter += OnProxyTriggerEnter;
        _nearbyTrigger.OnExit += OnProxyTriggerExit;

        _renderer = GetComponent<Renderer>();
        _material = _renderer.material;

        if (_selectMaterial is null)
        {
            _selectMaterial = (Material)Resources.Load("HighlightMaterial", typeof(Material));
        }

        OnAwake();
    }

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
        if (!IsSelectable || !_highlightOnHover)
        {
            state = HoverState.Deselected;
        }

        // TODO: highlight shader
        if (state == HoverState.Selected)
        {
            // enable highlight
            _renderer.material = _selectMaterial;
        }
        else
        {
            _renderer.material = _material;
            // disable highlight
        }
    }

    protected virtual void OnAwake() { }

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
    
    public bool IsInteractable {get; set; }
}
