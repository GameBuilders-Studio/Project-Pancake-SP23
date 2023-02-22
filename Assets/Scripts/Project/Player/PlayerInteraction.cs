using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private List<Selectable> _nearby;

    [Tooltip("Transform to place carried items in")]
    [SerializeField]
    private Transform _carryPivot;

    [Tooltip("Angle range in front of player to check for selectables")]
    [Range(0f, 180f)]
    [SerializeField]
    private float _selectAngleRange;

    private Selectable _hoverTarget = null;
    private Selectable _currentHeldItem = null;

    public List<Selectable> Nearby
    {
        get => _nearby;
        set => _nearby = value;
    }

    public Selectable HoverTarget
    {
        get => _hoverTarget;
        set => _hoverTarget = value;
    }

    public bool IsCarrying
    {
        get => _currentHeldItem != null;
    }

    void Awake()
    {
        Nearby = new();
    }

    void Update()
    {
        var selectable = GetBestSelectable();

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

        if (HoverTarget.IsCarryable)
        {
            Selectable item = HoverTarget.GetCarryableItem();
            item.OnPickUp();
            PickUpItem(item);
        }
    }

    public void TryPlace()
    {
        if (HoverTarget == null) { return; }

        if (HoverTarget.TryPlaceItem(_currentHeldItem))
        {
            _currentHeldItem = null;
            _currentHeldItem.OnPlace();
        }
    }

    public void PickUpItem(Selectable item)
    {
        if (_currentHeldItem != null)
        {
            Debug.LogError("Tried to pick up item while already carrying one");
            return;
        }
        _currentHeldItem = item;

        item.transform.SetParent(_carryPivot);
        
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    public void OnInteractStay()
    {
        if (HoverTarget == null) { return; }

        Debug.Log("tried to interact with this", HoverTarget);

        if (HoverTarget.Interactable == null) { return; }

        // do something with HoverTarget.Interactable
        HoverTarget.Interactable.Interact();
    }

    public void OnInteractEnd()
    {
        if (HoverTarget != null && HoverTarget.Interactable != null)
        {
            HoverTarget.Interactable.CancelInteract();
        }
    }

    /// <summary>
    /// Gets a selectable that the player is facing
    /// </summary>
    private Selectable GetBestSelectable()
    {
        Selectable nearest = null;

        float minAngle = Mathf.Infinity;
        
        for (int i = 0; i < Nearby.Count; i++)
        {
            float angle = Angle2D(transform.forward, Nearby[i].transform.position - transform.position);
            if (angle < minAngle && angle < _selectAngleRange)
            {
                nearest = Nearby[i];
                minAngle = angle;
            }
        }

        return nearest;
    }

    // TODO: make extension method
    private float Angle2D(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Angle(a, b);
    }
}
