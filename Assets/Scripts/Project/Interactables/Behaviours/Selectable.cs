using System;
using System.Collections.Generic;
using UnityEngine;
using CustomAttributes;

public enum HoverState { Selected, Deselected }

public enum SelectState { Default, Disabled }

public class Selectable : InteractionBehaviour
{
    [SerializeField]
    [Required]
    private ProxyTrigger _nearbyTrigger;

    [SerializeField]
    private HighlightBehaviour _highlightBehaviour;

    [SerializeField]
    private bool _highlightOnHover = true;

    [SerializeField]
    [ReadOnly]
    private bool _isSelectable = true;

    public static readonly Dictionary<GameObject, List<Selectable>> NearbyObjects = new();

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
    }

    void OnEnable()
    {
        _nearbyTrigger.Enter += OnProxyTriggerEnter;
        _nearbyTrigger.Exit += OnProxyTriggerExit;
    }

    void OnDisable()
    {
        _nearbyTrigger.Enter -= OnProxyTriggerEnter;
        _nearbyTrigger.Exit -= OnProxyTriggerExit;

        foreach (var managedList in NearbyObjects.Values)
        {
            managedList.Remove(this);
        }
    }

    public static void AddListener(GameObject gameObject, List<Selectable> managedList)
    {
        if (!NearbyObjects.TryAdd(gameObject, managedList))
        {
            Debug.LogWarning("Cannot add gameObject listener twice");
        }
    }

    public static void RemoveListener(GameObject gameObject)
    {
        NearbyObjects.Remove(gameObject);
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
        if (NearbyObjects.TryGetValue(other.gameObject, out List<Selectable> selectables))
        {
            selectables.Add(this);
        }
    }

    void OnProxyTriggerExit(Collider other)
    {
        if (NearbyObjects.TryGetValue(other.gameObject, out List<Selectable> selectables))
        {
            selectables.Remove(this);
        }
    }
}