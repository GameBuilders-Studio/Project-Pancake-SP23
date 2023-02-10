using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private List<Selectable> _nearby;

    [Tooltip("Angle range in front of player to check for selectables")]
    [SerializeField]
    private float _selectAngleRange;

    private Selectable _hoverTarget = null;

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

    void Awake()
    {
        Nearby = new();
    }

    void Update()
    {
        var selectable = GetBestSelectable();

        if (HoverTarget != null)
            HoverTarget.SetHoverState(HoverState.Deselected);

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

    public void TryPickUp()
    {
        if (HoverTarget == null)
            return;

        Debug.Log("tried to pick up this", HoverTarget);

        if (!HoverTarget.IsCarryable)
            return;

        // do something with HoverTarget
    }

    public void TryInteract()
    {
        if (HoverTarget == null)
            return;

        Debug.Log("tried to interact with this", HoverTarget);

        if (HoverTarget.Interactable == null)
            return;

        // do something with HoverTarget.Interactable
        HoverTarget.Interactable.Interact();
    }

    public void TryCancelInteract()
    {
        if (HoverTarget != null && HoverTarget.Interactable != null)
            HoverTarget.Interactable.CancelInteract();
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
            var angle = Angle2D(transform.forward, Nearby[i].transform.position - transform.position);
            if (angle < minAngle && angle < _selectAngleRange)
            {
                nearest = Nearby[i];
                minAngle = angle;
            }
        }

        return nearest;
    }

    float Angle2D(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Angle(a, b);
    }
}
