using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HoverState {Selected, Deselected}

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

    public bool IsSelectable
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
    }

    void OnProxyTriggerEnter(Collider other)
    {
        if (IsSelectable && other.gameObject.TryGetComponent<PlayerInteraction>(out PlayerInteraction player))
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

    public void SetHoverState(HoverState state)
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
}
