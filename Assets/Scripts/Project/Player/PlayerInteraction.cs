using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    void Awake()
    {
        Nearby = new();
    }

    void Update()
    {
        var selectable = GetBestSelectable();

        if (selectable != null)
        {
            _hoverTarget = selectable;
            _hoverTarget.SetHoverState(HoverState.Selected);
        }
        else
        {
            if (_hoverTarget != null)
                _hoverTarget.SetHoverState(HoverState.Deselected);

            _hoverTarget = null;
        }
    }

    public void TryPickUp()
    {
        if (_hoverTarget != null)
            Debug.Log("tried to pick up this", _hoverTarget);
    }

    public void TryInteract()
    {
        if (_hoverTarget != null)
            Debug.Log("tried to interact with this", _hoverTarget);
    }

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

    float Distance2D(Vector3 from, Vector3 to)
    {
        from.y = 0f;
        to.y = 0f;
        return Vector3.Distance(to, from);
    }

    float Angle2D(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Angle(a, b);
    }
}
