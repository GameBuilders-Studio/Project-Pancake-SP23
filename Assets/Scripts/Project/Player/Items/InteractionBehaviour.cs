using System;
using UnityEngine;
using CustomAttributes;

[RequireComponent(typeof(InteractableEntity))]
public abstract class InteractionBehaviour : MonoBehaviour
{
    [HideInInspector] public InteractableEntity Entity;
}

public interface IInteractionInterface {}

public interface ICombinable : IInteractionInterface
{
    public bool TryCombineWith(InteractableEntity other);
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