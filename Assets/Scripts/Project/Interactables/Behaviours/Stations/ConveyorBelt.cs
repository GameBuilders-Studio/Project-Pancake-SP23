using UnityEngine;
using DG.Tweening;
using CustomAttributes;
using System.Collections.Generic;

public class ConveyorBelt : StationController
{
    [SerializeField]
    [Min(0.0f)]
    private float _tweenTimeSeconds = 1.0f;

    [SerializeField]
    [Min(0.0f)]
    private float _tweenDistance = 1.0f;

    [SerializeField]
    [Required]
    private Transform _boxPivot;

    [SerializeField]
    private Vector3 _boxExtents;

    private Collider[] _overlapResults = new Collider[32];

    private bool _itemWasDestroyed = false;

    void OnDrawGizmosSelected()
    {
        if (_boxPivot == null) { return; }
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_boxPivot.position, _boxExtents);
    }

    public override bool ValidateItem(Carryable carryable)
    {
        _itemWasDestroyed = false;

        if (!TryGetNearestItem(out Carryable item, carryable.gameObject)) 
        {
            return true; 
        }

        if (item.TryGetInterface(out ICombinable combinable))
        {
            _itemWasDestroyed = combinable.TryCombineWith(carryable);
            return _itemWasDestroyed;
        }

        // Combining 
        if (carryable.TryGetInterface(out ICombinable otherCombinable))
        {
            //var tweener = item.CurrentTweener;
            if (otherCombinable.TryCombineWith(item))
            {
                return true;
            }
        }

        return false;
    }

    public override void ItemPlaced(ref Carryable item)
    {
        if (_itemWasDestroyed) { return; }

        item.DisablePhysics();
        item.DisableSelection();

        var targetPosition = item.transform.position + (transform.forward * _tweenDistance);

        var tweenerCore = item.transform
            .DOMove(targetPosition, _tweenTimeSeconds)
            .SetEase(Ease.Linear);

        Debug.Log($"assigned tween to object {item.gameObject.name}");

        tweenerCore.onComplete += item.OnDrop;
        item.CurrentTweener = tweenerCore;
        
        item = null;
    }

    public override void PositionItem(ref Carryable item, Transform pivot)
    {
        if (_itemWasDestroyed) { return; }
        base.PositionItem(ref item, pivot);
    }

    public override void ItemRemoved(ref Carryable carryable)
    {
        if (TryGetNearestItem(out Carryable item))
        {
            carryable = item;
        }
    }

    private bool TryGetNearestItem(out Carryable item, GameObject ignoredObject = null)
    {
        item = default;
        bool itemExists = false;

        int overlaps = Physics.OverlapBoxNonAlloc(_boxPivot.position, _boxExtents, _overlapResults, _boxPivot.rotation, -1, QueryTriggerInteraction.Ignore);

        float minDistance = Mathf.Infinity;

        for (int i = 0; i < overlaps; i++)
        {
            var go = _overlapResults[i].gameObject;

            if (go == ignoredObject) { continue; }

            if (Carryable.TryGetCarryable(go, out Carryable carryable))
            {
                itemExists = true;
                float distance = Vector3.Distance(go.transform.position, _boxPivot.position);
                if (distance < minDistance)
                {
                    item = carryable;
                    minDistance = distance;
                }
            }
        }

        return itemExists;
    }
}