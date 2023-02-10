using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum HoverState {Selected, Deselected}

public class Selectable : MonoBehaviour
{
    [SerializeField]
    private ProxyTriggerVolume _nearbyTrigger;

    [Space(15f)]
    [SerializeField]
    private bool _isSelectable = true;

    [SerializeField]
    private bool _highlightOnHover = true;

    private Dictionary<GameObject, PlayerInteraction> _nearbyPlayers = new();

    public bool IsSelectable
    {
        get => _isSelectable;
        set => _isSelectable = value;
    }

    void Awake()
    {
        if (_nearbyTrigger == null)
            _nearbyTrigger = GetComponentInChildren<ProxyTriggerVolume>();

        if (_nearbyTrigger == null)
            Debug.LogError("No proxy trigger volume");

        _nearbyTrigger.OnEnter += OnProxyTriggerEnter;
        _nearbyTrigger.OnExit += OnProxyTriggerExit;
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
        if (!IsSelectable)
            state = HoverState.Deselected;

        if (_highlightOnHover && state == HoverState.Selected)
        {
            // enable highlight
        }
        else
        {
            // disable highlight
        }
    }
}
