using System;
using UnityEngine;
using CustomAttributes;

[RequireComponent(typeof(InteractableEntity))]
public abstract class InteractionBehaviour : MonoBehaviour
{
    [SerializeField]
    [HideInInspector]
    public InteractableEntity Entity;
    
    // public bool TryGetBehaviour<T>(out T component) where T : InteractionBehaviour
    // {
    //     bool exists = Entity.TryGetBehaviour(out T behaviour);
    //     component = behaviour;
    //     return exists;
    // }

    // public bool HasBehaviour<T>() where T : InteractionBehaviour
    // {
    //     return Entity.HasBehaviour<T>();
    // }

    // public bool TryGetInterface<T>(out T _interface) where T : IInteractionInterface
    // {
    //     bool exists = Entity.TryGetInterface(out T interfaceOut);
    //     _interface = interfaceOut;
    //     return exists;
    // }

    // public bool HasInterface<T>() where T : IInteractionInterface
    // {
    //     return Entity.HasInterface<T>();
    // }
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