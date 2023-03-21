using System;
using UnityEngine;
using CustomAttributes;

[RequireComponent(typeof(InteractableEntity))]
public abstract class InteractionBehaviour : InteractionProvider
{
    [SerializeField]
    [HideInInspector]
    private InteractableEntity _entity;

    protected override InteractableEntity Entity => _entity;

    public void AssignToEntity(InteractableEntity entity)
    {
        _entity = entity;
    }
}

public interface IInteractionInterface {}

public interface ICombinable : IInteractionInterface
{
    public bool TryCombineWith(InteractionProvider other);
}

public interface IUsable : IInteractionInterface
{
    public void OnUseStart();
    public void OnUseEnd();
    public bool Enabled {get;}
}

public interface IHasCarryable : IInteractionInterface
{
    public Carryable PopCarryable();
}