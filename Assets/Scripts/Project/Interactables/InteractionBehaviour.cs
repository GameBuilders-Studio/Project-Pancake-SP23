using System;
using UnityEngine;
using CustomAttributes;

[RequireComponent(typeof(InteractionCollection))]
public abstract class InteractionBehaviour : InteractionProvider
{
    [SerializeField]
    [HideInInspector]
    private InteractionCollection _entity;

    protected override InteractionCollection Collection => _entity;

    public void AssignToEntity(InteractionCollection entity)
    {
        _entity = entity;
    }
}