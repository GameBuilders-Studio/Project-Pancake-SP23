using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using CustomAttributes;

public class ConveyorBelt : StationController
{
    [SerializeField]
    [Min(0.0f)]
    private float _conveyTimeSeconds = 1.0f;

    [SerializeField]
    [Min(0.0f)]
    private float _distance = 1.0f;

    [SerializeField]
    [Required]
    private Transform _pivotPoint;

    [SerializeField]
    private Vector3 _extents;

    public override void ItemPlaced(ref Carryable item)
    {
        item.DisablePhysics();
        item.EnableSelection();

        var targetPosition = item.transform.position + (transform.forward * _distance);

        var tweenerCore = item.transform
            .DOMove(targetPosition, _conveyTimeSeconds)
            .SetEase(Ease.Linear);

        tweenerCore.onComplete += item.EnablePhysics;

        item.CurrentTweener = tweenerCore;

        item = null;
    }

    public override bool ValidateItem(Carryable item)
    {

        return true;
    }
}
